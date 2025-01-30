using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Persistence.Queries;

/// <summary>
/// Represents an interface for executing queries against a database and retrieving results.
/// Provides methods for both single and multiple result query execution with support for optional parameters and transactions.
/// </summary>
public interface IQueryExecutor
{
    /// <summary>
    /// Executes an asynchronous query and retrieves a collection of results of type TResult.
    /// </summary>
    /// <typeparam name="TResult">The type of the result objects to retrieve. It only accepts DTO objects with writable properties.</typeparam>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">The parameters to pass to the query. Can be null if no parameters are required.</param>
    /// <param name="dbTransaction">The transaction within which the query should be executed. Can be null.</param>
    /// <param name="cancellationToken">A token to cancel the query.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a collection of TResult objects.
    /// </returns>
    public Task<ICollection<TResult>> QueryAsync<TResult>(string query, object? parameters, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an asynchronous query and retrieves a collection of results of type TResult.
    /// </summary>
    /// <typeparam name="TResult">The type of the result objects to retrieve. It only accepts DTO object with writable properties.</typeparam>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="dbTransaction">The transaction within which the query should be executed. Can be null.</param>
    /// <param name="cancellationToken">A token to cancel the query.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains an enumerable collection of TResult objects.
    /// </returns>
    public Task<ICollection<TResult>> QueryAsync<TResult>(string query, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
    {
        return QueryAsync<TResult>(query, null, dbTransaction, cancellationToken);
    }

    /// <summary>
    /// Executes an asynchronous query and retrieves a single result of type TResult.
    /// </summary>
    /// <typeparam name="TResult">The type of the result object to retrieve. It must be a DTO object with writable properties.</typeparam>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="dbTransaction">The transaction within which the query should be executed. Can be null.</param>
    /// <param name="cancellationToken">A token to cancel the query execution.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains a single TResult object.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the query does not return exactly one result.</exception>
    public Task<TResult> QuerySingleAsync<TResult>(string query, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
    {
        return QuerySingleAsync<TResult>(query, null, dbTransaction, cancellationToken);
    }

    /// <summary>
    /// Executes an asynchronous query and retrieves a single result of type TResult, or the default value for TResult if no result is found.
    /// </summary>
    /// <typeparam name="TResult">The type of the result object to retrieve. It only accepts DTO object with writable properties.</typeparam>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="dbTransaction">The transaction within which the query should be executed. Can be null.</param>
    /// <param name="cancellationToken">A token to cancel the query.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the single TResult object retrieved from the query, or the default value for TResult if no result is found.
    /// </returns>
    public Task<TResult?> QuerySingleOrDefaultAsync<TResult>(string query, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
    {
        return QuerySingleOrDefaultAsync<TResult>(query, null, dbTransaction, cancellationToken);
    }

    /// <summary>
    /// Executes an asynchronous query and retrieves a single result of type TResult.
    /// </summary>
    /// <typeparam name="TResult">The type of the result object to retrieve. It only accepts DTO objects with writable properties.</typeparam>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">Optional parameters to include in the query. Can be null.</param>
    /// <param name="dbTransaction">The transaction within which the query should be executed. Can be null.</param>
    /// <param name="cancellationToken">A token to cancel the query.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the single TResult object retrieved from the query.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the query does not return exactly one result.</exception>
    public async Task<TResult> QuerySingleAsync<TResult>(string query, object? parameters, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
    {
        var results = await QueryAsync<TResult>(query, parameters, dbTransaction, cancellationToken);
        return results.Single();
    }

    /// <summary>
    /// Executes an asynchronous query and retrieves a single result of type TResult, or the default value for TResult if no result is found.
    /// </summary>
    /// <typeparam name="TResult">The type of the result object to retrieve. It only accepts DTO objects with writable properties.</typeparam>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">An object containing parameter values to be passed to the query. Can be null.</param>
    /// <param name="dbTransaction">The transaction within which the query should be executed. Can be null.</param>
    /// <param name="cancellationToken">A token to cancel the query.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the single instance of TResult if one is found, otherwise the default value for TResult.
    /// </returns>
    public async Task<TResult?> QuerySingleOrDefaultAsync<TResult>(string query, object? parameters, IDbTransaction? dbTransaction = null, CancellationToken cancellationToken = default)
    {
        var results = await QueryAsync<TResult>(query, parameters, dbTransaction, cancellationToken);
        return results.SingleOrDefault();
    }
}