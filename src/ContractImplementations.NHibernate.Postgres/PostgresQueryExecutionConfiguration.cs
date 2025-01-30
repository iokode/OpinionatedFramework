using IOKode.OpinionatedFramework.Utilities;
using NHibernate.Transform;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres;

public class PostgresQueryExecutionConfiguration : IQueryExecutorConfiguration
{
    public IResultTransformer GetResultTransformer<TResult>()
    {
        return new SnakeCaseAliasToBeanResultTransformer<TResult>();
    }

    public string TransformAlias(string alias)
    {
        return StringUtility.ToSnakeCase(alias);
    }

    public string TransformParameterName(string parameterName)
    {
        return StringUtility.ToSnakeCase(parameterName);
    }
}