using System;
using FluentNHibernate.Cfg;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres.Mappings;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres.UserTypes.NodaTime;
using NodaTime;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres;

public static class ServiceExtensions
{
    public static void AddNHibernateWithPostgres(this IOpinionatedServiceCollection services, Action<global::NHibernate.Cfg.Configuration> configuration)
    {
        var queryExecutorConfig = new PostgresQueryExecutorConfiguration();
        services.AddNHibernate(configuration, queryExecutorConfig);
        AddOpinionatedFrameworkPostgresUserTypes();
    }

    public static void AddOpinionatedFrameworkPostgresMappings(this FluentMappingsContainer container)
    {
        container.Add<EventMap>();
    }

    public static void AddOpinionatedFrameworkPostgresUserTypes()
    {
        UserTypeMapper.AddUserType<Instant, InstantUserType>();
    }
}