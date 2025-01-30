using System;
using IOKode.OpinionatedFramework.Bootstrapping;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres;

public static class ServiceExtensions
{
    public static void AddNHibernateWithPostgres(this IOpinionatedServiceCollection services, Action<global::NHibernate.Cfg.Configuration> configuration)
    {
        var queryExecutorConfig = new PostgresQueryExecutionConfiguration();
        services.AddNHibernate(configuration, queryExecutorConfig);
    }
}