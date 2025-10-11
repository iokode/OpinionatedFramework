using Humanizer;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.QueryExecutor;
using NHibernate.Transform;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres;

public class PostgresQueryExecutorConfiguration : IQueryExecutorConfiguration
{
    public IResultTransformer GetResultTransformer<TResult>()
    {
        return new SnakeCaseAliasToBeanResultTransformer<TResult>();
    }

    public string TransformAlias(string alias)
    {
        return alias.Underscore();
    }

    public string TransformParameterName(string parameterName)
    {
        return parameterName.Underscore();
    }
}