namespace IOKode.OpinionatedFramework.Persistence.Queries;

public interface IQueryExecutorFactory
{
    public IQueryExecutor Create(params QueryMiddleware[] middlewares);
}