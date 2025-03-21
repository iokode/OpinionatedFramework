using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Persistence.Queries;

public delegate Task InvokeNextMiddlewareDelegate();

public abstract class QueryMiddleware
{
    public abstract Task ExecuteAsync(IQueryExecutionContext executionContext, InvokeNextMiddlewareDelegate nextAsync);
}