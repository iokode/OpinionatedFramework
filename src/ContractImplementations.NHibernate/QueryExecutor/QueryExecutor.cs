using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.Persistence.Queries;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Exceptions;
using NHibernate;
using NHibernate.Type;
using NonUniqueResultException = IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Exceptions.NonUniqueResultException;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.QueryExecutor;

public partial class QueryExecutor(
    ISessionFactory sessionFactory,
    IQueryExecutorConfiguration configuration,
    params QueryMiddleware[] middlewares) : IQueryExecutor
{
    private enum ResultCardinality
    {
        Multiple,
        Single,
        SingleOrDefault
    }

    public async Task<IReadOnlyCollection<TResult>> QueryAsync<TResult>(string query, object? parameters,
        IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
    {
        return await QueryAsync<TResult>(ResultCardinality.Multiple, query, parameters, dbTransaction, cancellationToken);
    }

    public async Task<TResult> QuerySingleAsync<TResult>(string query, object? parameters, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
    {
        var results = await QueryAsync<TResult>(ResultCardinality.Single, query, parameters, dbTransaction, cancellationToken);
        return results.FirstOrDefault()!; // Exception related to single already thrown by QueryAsync
    }

    public async Task<TResult?> QuerySingleOrDefaultAsync<TResult>(string query, object? parameters, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
    {
        var results = await QueryAsync<TResult>(ResultCardinality.SingleOrDefault, query, parameters, dbTransaction, cancellationToken);
        return results.FirstOrDefault(); // Exception related to single or default already thrown by QueryAsync
    }

    private async Task<IReadOnlyCollection<TResult>> QueryAsync<TResult>(ResultCardinality resultCardinality, string query, object? parameters,
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

        await InvokeMiddlewarePipelineAsync<TResult>(resultCardinality, context, 0);

        Log.Trace("Invoked query pipeline.");
        Container.Advanced.DisposeScope();

        var results = context.Results.Cast<TResult>().ToList();
        return results;
    }

    private void ExtractDirectivesFromQuery(NHibernateQueryExecutionExecutorContext context)
    {
        var lines = context.RawQuery.Split('\n');
        var directivePrefixRegex = GetDirectivePrefixRegex();

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            var match = directivePrefixRegex.Match(trimmedLine);
            if (!match.Success)
            {
                continue;
            }

            string directive = trimmedLine[match.Length..].Trim();
            context.Directives.Add(directive);
        }
    }

    private async Task InvokeMiddlewarePipelineAsync<TResult>(ResultCardinality resultCardinality,
        NHibernateQueryExecutionExecutorContext context, int index)
    {
        if (index >= middlewares.Length)
        {
            await ExecuteQueryAsync<TResult>(resultCardinality, context);
            return;
        }

        var middleware = middlewares[index];
        await middleware.ExecuteAsync(context, () => InvokeMiddlewarePipelineAsync<TResult>(resultCardinality, context, index + 1));
    }

    private async Task ExecuteQueryAsync<TResult>(ResultCardinality resultCardinality, NHibernateQueryExecutionExecutorContext context)
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

        if (resultCardinality != ResultCardinality.Multiple)
        {
            try
            {
                var singleResult = context.Results.SingleOrDefault();
                if (singleResult == null && resultCardinality == ResultCardinality.Single)
                {
                    throw new EmptyResultException();
                }
            }
            catch (InvalidOperationException exc)
            {
                throw new NonUniqueResultException(exc);
            }
        }
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

    [GeneratedRegex(@"--[ \t]*@")]
    private static partial Regex GetDirectivePrefixRegex();
}