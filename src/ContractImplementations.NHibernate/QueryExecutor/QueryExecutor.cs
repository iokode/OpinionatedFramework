using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.ServiceContainer;
using IOKode.OpinionatedFramework.Facades;
using IOKode.OpinionatedFramework.Persistence.Queries;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Exceptions;
using IOKode.OpinionatedFramework.Utilities;
using NHibernate;
using NHibernate.Multi;
using NHibernate.Transform;
using NHibernate.Type;
using NonUniqueResultException = IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Exceptions.NonUniqueResultException;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.QueryExecutor;

public class QueryExecutor(
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

    public async Task<IReadOnlyList<object>> QueryResultSetsAsync(IReadOnlyList<QueryResultSet> resultSets, IReadOnlyList<string> directives, object? parameters,
        IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
    {
        var context = new NHibernateQueryExecutionExecutorContext
        {
            CancellationToken = cancellationToken,
            Directives = directives.ToList(),
            HasMultipleResultSets = resultSets.Count > 1,
            Parameters = parameters,
            RawQuery = string.Join(Environment.NewLine, resultSets.Select(resultSet => resultSet.RawSql)),
            ResultSets = CreateResultSetExecutionContexts(resultSets),
            Results = new List<object>(),
            Transaction = dbTransaction,
            IsExecuted = false,
            TraceID = Guid.NewGuid()
        };
        Container.Advanced.CreateScope();
        Log.Trace("Invoking query pipeline...");

        await InvokeResultSetsMiddlewarePipelineAsync(resultSets, context, 0);

        Log.Trace("Invoked query pipeline.");
        Container.Advanced.DisposeScope();

        return context.IsExecuted ? context.Results : Array.Empty<object>();
    }

    private async Task<IReadOnlyCollection<TResult>> QueryAsync<TResult>(ResultCardinality resultCardinality, string query, object? parameters,
        IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
    {
        var context = new NHibernateQueryExecutionExecutorContext
        {
            CancellationToken = cancellationToken,
            Directives = SqlUtilities.GetDirectives(query).ToList(),
            HasMultipleResultSets = false,
            Parameters = parameters,
            RawQuery = query,
            ResultSets =
            [
                new NHibernateQueryResultSetExecutionContext
                {
                    Name = null,
                    Directives = SqlUtilities.GetDirectives(query),
                    Cardinality = resultCardinality switch
                    {
                        ResultCardinality.Single => QueryCardinality.One,
                        ResultCardinality.SingleOrDefault => QueryCardinality.ZeroOrOne,
                        _ => QueryCardinality.ZeroOrMore
                    },
                    Shape = QueryResultShape.Object,
                    ResultType = typeof(TResult),
                    ScalarColumnName = null,
                    RawQuery = query,
                }
            ],
            Results = new List<object>(),
            Transaction = dbTransaction,
            IsExecuted = false,
            TraceID = Guid.NewGuid()
        };
        Container.Advanced.CreateScope();
        Log.Trace("Invoking query pipeline...");

        await InvokeMiddlewarePipelineAsync<TResult>(resultCardinality, context, 0);

        Log.Trace("Invoked query pipeline.");
        Container.Advanced.DisposeScope();

        var results = context.IsExecuted ? context.Results.Cast<TResult>().ToList() : new List<TResult>();
        return results;
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

    private async Task InvokeResultSetsMiddlewarePipelineAsync(IReadOnlyList<QueryResultSet> resultSets,
        NHibernateQueryExecutionExecutorContext context, int index)
    {
        if (index >= middlewares.Length)
        {
            await ExecuteResultSetsQueryAsync(resultSets, context);
            return;
        }

        var middleware = middlewares[index];
        await middleware.ExecuteAsync(context, () => InvokeResultSetsMiddlewarePipelineAsync(resultSets, context, index + 1));
    }

    private async Task ExecuteQueryAsync<TResult>(ResultCardinality resultCardinality, NHibernateQueryExecutionExecutorContext context)
    {
        using var session = context.Transaction == null
            ? sessionFactory.OpenStatelessSession()
            : sessionFactory.OpenStatelessSession(context.Transaction.Connection as DbConnection);

        var resultSet = context.ResultSets.Single();
        var sqlQuery = session.CreateSQLQuery(resultSet.RawQuery);

        AddScalarsForType<TResult>(sqlQuery);
        sqlQuery.SetResultTransformer(configuration.GetResultTransformer<TResult>());

        if (context.Parameters != null)
        {
            AddParameters(sqlQuery, context.Parameters, resultSet.RawQuery, false);
        }

        var results = (await sqlQuery.ListAsync<object>(context.CancellationToken)).ToArray();
        context.Results = results;
        ((NHibernateQueryResultSetExecutionContext)resultSet).Results = results;
        context.IsExecuted = true;

        if (resultCardinality is ResultCardinality.Single or ResultCardinality.SingleOrDefault && results.Length > 1)
        {
            throw new NonUniqueResultException();
        }

        if (resultCardinality is ResultCardinality.Single && results.Length == 0)
        {
            throw new EmptyResultException();
        }
    }

    private async Task ExecuteResultSetsQueryAsync(IReadOnlyList<QueryResultSet> resultSets, NHibernateQueryExecutionExecutorContext context)
    {
        using var session = context.Transaction == null
            ? sessionFactory.OpenStatelessSession()
            : sessionFactory.OpenStatelessSession(context.Transaction.Connection as DbConnection);

        resultSets = CreateResultSetsFromExecutionContext(context);

        if (resultSets.Count == 1)
        {
            var result = await ExecuteSingleResultSetQueryAsync(session, resultSets[0], context);
            context.Results = new[] { result };
            context.IsExecuted = true;
            return;
        }

        var batch = session.CreateQueryBatch();
        for (int i = 0; i < resultSets.Count; i++)
        {
            var resultSet = resultSets[i];
            var sqlQuery = session.CreateSQLQuery(resultSet.RawSql);
            ConfigureResultSetQuery(sqlQuery, resultSet);

            if (context.Parameters != null)
            {
                AddParameters(sqlQuery, context.Parameters, resultSet.RawSql, true);
            }

            AddQueryToBatch(batch, i.ToString(), resultSet.ResultType, sqlQuery);
        }

        await batch.ExecuteAsync(context.CancellationToken);

        var results = new List<object>();
        for (int i = 0; i < resultSets.Count; i++)
        {
            var resultSet = resultSets[i];
            var result = await GetBatchResultAsync(batch, i, resultSet.ResultType, context.CancellationToken);
            EnsureCardinality(resultSet.Cardinality, GetCount(result));
            results.Add(result);
            ((NHibernateQueryResultSetExecutionContext)context.ResultSets[i]).Results = ((System.Collections.IEnumerable)result).Cast<object>().ToList();
        }

        context.Results = results;
        context.IsExecuted = true;
    }

    private static IReadOnlyList<IQueryResultSetExecutionContext> CreateResultSetExecutionContexts(IReadOnlyList<QueryResultSet> resultSets)
    {
        return resultSets
            .Select(resultSet => new NHibernateQueryResultSetExecutionContext
            {
                Name = resultSet.Name,
                Directives = resultSet.Directives,
                Cardinality = resultSet.Cardinality,
                Shape = resultSet.Shape,
                ResultType = resultSet.ResultType,
                ScalarColumnName = resultSet.ScalarColumnName,
                RawQuery = resultSet.RawSql,
            })
            .ToList();
    }

    private static IReadOnlyList<QueryResultSet> CreateResultSetsFromExecutionContext(NHibernateQueryExecutionExecutorContext context)
    {
        return context.ResultSets
            .Select(resultSet => new QueryResultSet
            {
                Name = resultSet.Name,
                RawSql = resultSet.RawQuery,
                Directives = resultSet.Directives,
                Cardinality = resultSet.Cardinality,
                Shape = resultSet.Shape,
                ResultType = resultSet.ResultType,
                ScalarColumnName = resultSet.ScalarColumnName,
            })
            .ToList();
    }

    private async Task<object> ExecuteSingleResultSetQueryAsync(IStatelessSession session, QueryResultSet resultSet,
        NHibernateQueryExecutionExecutorContext context)
    {
        var method = typeof(QueryExecutor)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(method => method.Name == nameof(ExecuteSingleResultSetQueryAsync)
                              && method.IsGenericMethodDefinition
                              && method.GetParameters().Length == 3);

        var task = (Task<object>)method.MakeGenericMethod(resultSet.ResultType).Invoke(this, new object[] { session, resultSet, context })!;
        return await task;
    }

    private async Task<object> ExecuteSingleResultSetQueryAsync<TResult>(IStatelessSession session, QueryResultSet resultSet,
        NHibernateQueryExecutionExecutorContext context)
    {
        var sqlQuery = session.CreateSQLQuery(resultSet.RawSql);
        ConfigureResultSetQuery(sqlQuery, resultSet);

        if (context.Parameters != null)
        {
            AddParameters(sqlQuery, context.Parameters, resultSet.RawSql, false);
        }

        var results = (await sqlQuery.ListAsync<TResult>(context.CancellationToken)).ToList();
        EnsureCardinality(resultSet.Cardinality, results.Count);
        ((NHibernateQueryResultSetExecutionContext)context.ResultSets.Single()).Results = results.Cast<object>().ToList();
        return results;
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

    private void ConfigureResultSetQuery(ISQLQuery query, QueryResultSet resultSet)
    {
        if (resultSet.Shape == QueryResultShape.Scalar)
        {
            if (string.IsNullOrWhiteSpace(resultSet.ScalarColumnName))
            {
                throw new QueryDefinitionException("Scalar result sets must declare a scalar column name.");
            }

            query.AddScalar(configuration.TransformAlias(resultSet.ScalarColumnName), GetNHTypeFor(resultSet.ResultType));
            return;
        }

        AddScalarsForType(query, resultSet.ResultType);
        query.SetResultTransformer(GetResultTransformer(resultSet.ResultType));
    }

    private void AddScalarsForType(ISQLQuery query, Type resultType)
    {
        var props = resultType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToList();

        foreach (var prop in props)
        {
            string alias = configuration.TransformAlias(prop.Name);
            var nhType = GetNHTypeFor(prop.PropertyType);
            query.AddScalar(alias, nhType);
        }
    }

    private IResultTransformer GetResultTransformer(Type resultType)
    {
        var method = typeof(IQueryExecutorConfiguration).GetMethod(nameof(IQueryExecutorConfiguration.GetResultTransformer))!;
        return (IResultTransformer)method.MakeGenericMethod(resultType).Invoke(configuration, Array.Empty<object?>())!;
    }

    /// <summary>
    /// Maps a .NET type to the corresponding NHibernate IType. Uses either custom mappings
    /// defined in ConventionMaps or NHibernate's built-in type guessing mechanism.
    /// </summary>
    /// <param name="propertyType">The .NET type for which the NHibernate IType is being resolved.</param>
    /// <returns>The NHibernate IType that corresponds to the specified .NET type.</returns>
    /// <exception cref="QueryDefinitionException">Thrown if the specified .NET type cannot be resolved to a supported NHibernate IType.</exception>
    private static IType GetNHTypeFor(Type propertyType)
    {
        propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        var nhUserType = UserTypeMapper.GetNhUserType(propertyType);
        if (nhUserType != null)
        {
            return NHibernateUtil.Custom(nhUserType);
        }

        // Fallback: Use NHibernateUtil to resolve built-in types or throw an exception for unsupported types.
        return NHibernateUtil.GuessType(propertyType) ??
               throw new QueryDefinitionException($"Unsupported query type: {propertyType.FullName}");
    }

    /// <summary>
    /// Simple reflection-based parameter setter. 
    /// If your "parameters" object has properties that match named parameters in the SQL (e.g. :id), 
    /// you can set them.
    /// </summary>
    private void AddParameters(ISQLQuery query, object parameters, string rawQuery, bool ignoreMissingParameters)
    {
        var paramProps = parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in paramProps)
        {
            var paramName = configuration.TransformParameterName(prop.Name);
            if (ignoreMissingParameters && !rawQuery.Contains($":{paramName}"))
            {
                continue;
            }

            var value = prop.GetValue(parameters, null);
            query.SetParameter(paramName, value, GetNHTypeFor(prop.PropertyType));
        }
    }

    private static void AddQueryToBatch(IQueryBatch batch, string key, Type resultType, IQuery query)
    {
        var method = typeof(QueryBatchExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method => method.Name == nameof(QueryBatchExtensions.Add)
                              && method.IsGenericMethodDefinition
                              && method.GetParameters().Length == 3
                              && method.GetParameters()[0].ParameterType == typeof(IQueryBatch)
                              && method.GetParameters()[1].ParameterType == typeof(string)
                              && method.GetParameters()[2].ParameterType == typeof(IQuery));

        method.MakeGenericMethod(resultType).Invoke(null, new object[] { batch, key, query });
    }

    private static async Task<object> GetBatchResultAsync(IQueryBatch batch, int index, Type resultType, CancellationToken cancellationToken)
    {
        var method = typeof(IQueryBatch)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(method => method.Name == nameof(IQueryBatch.GetResultAsync)
                              && method.IsGenericMethodDefinition
                              && method.GetParameters().Length == 2
                              && method.GetParameters()[0].ParameterType == typeof(int));

        var task = (Task)method.MakeGenericMethod(resultType).Invoke(batch, new object[] { index, cancellationToken })!;
        await task;

        return task.GetType().GetProperty(nameof(Task<object>.Result))!.GetValue(task)!;
    }

    private static void EnsureCardinality(QueryCardinality cardinality, int resultCount)
    {
        if (cardinality is QueryCardinality.One or QueryCardinality.ZeroOrOne && resultCount > 1)
        {
            throw new NonUniqueResultException();
        }

        if (cardinality is QueryCardinality.One && resultCount == 0)
        {
            throw new EmptyResultException();
        }
    }

    private static int GetCount(object result)
    {
        return (int)result.GetType().GetProperty(nameof(IReadOnlyCollection<object>.Count))!.GetValue(result)!;
    }
}
