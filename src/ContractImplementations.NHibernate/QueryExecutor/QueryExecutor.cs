using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.Persistence.Queries;
using NHibernate;
using NHibernate.Type;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.QueryExecutor;

public class QueryExecutor(
    ISessionFactory sessionFactory,
    IQueryExecutorConfiguration configuration,
    params QueryMiddleware[] middlewares) : IQueryExecutor
{
    public async Task<IReadOnlyCollection<TResult>> QueryAsync<TResult>(string query, object? parameters,
        IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
    {
        var context = new NHibernateQueryExecutionExecutorContext
        {
            CancellationToken = cancellationToken,
            Directives = new List<string>(),
            Parameters = parameters,
            RawQuery = query,
            Results = new List<object>(),
            Transaction = dbTransaction,
            IsExecuted = false,
            TraceID = Guid.NewGuid()
        };
        ExtractDirectivesFromQuery(context);

        Container.Advanced.CreateScope();
        Log.Trace("Invoking query pipeline...");

        await InvokeMiddlewarePipelineAsync<TResult>(context, 0);

        Log.Trace("Invoked query pipeline.");
        Container.Advanced.DisposeScope();

        var results = context.Results.Cast<TResult>().ToList();
        return results;
    }

    private void ExtractDirectivesFromQuery(NHibernateQueryExecutionExecutorContext context)
    {
        var lines = context.RawQuery.Split('\n');

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("-- @"))
            {
                string directive = trimmedLine[4..].Trim();
                context.Directives.Add(directive);
            }
        }
    }

    private async Task InvokeMiddlewarePipelineAsync<TResult>(NHibernateQueryExecutionExecutorContext context,
        int index)
    {
        if (index >= middlewares.Length)
        {
            await ExecuteQueryAsync<TResult>(context);
            return;
        }

        var middleware = middlewares[index];
        await middleware.ExecuteAsync(context, () => InvokeMiddlewarePipelineAsync<TResult>(context, index + 1));
    }

    private async Task ExecuteQueryAsync<TResult>(NHibernateQueryExecutionExecutorContext context)
    {
        using var session = context.Transaction == null
            ? sessionFactory.OpenStatelessSession()
            : sessionFactory.OpenStatelessSession(context.Transaction.Connection as DbConnection);

        var sqlQuery = session.CreateSQLQuery(context.RawQuery);

        AddScalarsForType<TResult>(sqlQuery);
        sqlQuery.SetResultTransformer(configuration.GetResultTransformer<TResult>());

        if (context.Parameters != null)
        {
            AddParameters(sqlQuery, context.Parameters);
        }

        context.Results = (await sqlQuery.ListAsync<object>(context.CancellationToken)).ToArray();
        context.IsExecuted = true;
    }

    /// <summary>
    /// Loops through properties of TResult, adding .AddScalar for each
    /// using either a custom IUserType for certain property types
    /// or standard NHibernateUtil fallback for others.
    /// </summary>
    private void AddScalarsForType<TResult>(ISQLQuery query)
    {
        var props = typeof(TResult).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite) // only map settable props
            .ToList();

        foreach (var prop in props)
        {
            // We'll use the property name as the "column alias". 
            // That means your SQL must do `SELECT Column AS property_name ...`
            // for each property you want to map.
            string alias = configuration.TransformAlias(prop.Name);

            var nhType = GetNHTypeFor(prop.PropertyType);
            query.AddScalar(alias, nhType);
        }
    }

    /// <summary>
    /// Maps a .NET type to the corresponding NHibernate IType. Uses either custom mappings
    /// defined in ConventionMaps or NHibernate's built-in type guessing mechanism.
    /// </summary>
    /// <param name="propertyType">The .NET type for which the NHibernate IType is being resolved.</param>
    /// <returns>The NHibernate IType that corresponds to the specified .NET type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified .NET type cannot be resolved to a supported NHibernate IType.</exception>
    private static IType GetNHTypeFor(Type propertyType)
    {
        var nhUserType = UserTypeMapper.GetNhUserType(propertyType);
        if (nhUserType != null)
        {
            return NHibernateUtil.Custom(nhUserType);
        }

        // Fallback: Use NHibernateUtil to resolve built-in types or throw an exception for unsupported types.
        return NHibernateUtil.GuessType(propertyType) ??
               throw new InvalidOperationException($"Unsupported type: {propertyType.FullName}");
    }

    /// <summary>
    /// Simple reflection-based parameter setter. 
    /// If your "parameters" object has properties that match named parameters in the SQL (e.g. :id), 
    /// you can set them.
    /// </summary>
    private void AddParameters(ISQLQuery query, object parameters)
    {
        var paramProps = parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in paramProps)
        {
            var paramName = configuration.TransformParameterName(prop.Name);
            var value = prop.GetValue(parameters, null);
            query.SetParameter(paramName, value, GetNHTypeFor(prop.PropertyType));
        }
    }
}