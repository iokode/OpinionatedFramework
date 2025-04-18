using System;
using FluentNHibernate.Cfg;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres.Mappings;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres;

public static class ServiceExtensions
{
    public static void AddNHibernateWithPostgres(this IOpinionatedServiceCollection services, Action<global::NHibernate.Cfg.Configuration> configuration)
    {
        var queryExecutorConfig = new PostgresQueryExecutorConfiguration();
        services.AddNHibernate(configuration, queryExecutorConfig);
    }

    public static void AddOpinionatedFrameworkPostgresMappings(this FluentMappingsContainer container)
    {
        container.Add<EventMap>();
    }
}