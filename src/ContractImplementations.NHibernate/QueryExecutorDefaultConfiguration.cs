using NHibernate.Transform;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate;

public class QueryExecutorDefaultConfiguration : IQueryExecutorConfiguration
{
    public IResultTransformer GetResultTransformer<TResult>()
    {
        return new AliasToBeanResultTransformer(typeof(TResult));
    }

    public string TransformAlias(string alias)
    {
        return alias;
    }

    public string TransformParameterName(string parameterName)
    {
        return parameterName;
    }
}