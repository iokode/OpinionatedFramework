using System;
using FluentNHibernate.Cfg;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres.Mappings;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres.UserTypes.NodaTime;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.QueryExecutor;
using NodaTime;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres;

public static class ServiceExtensions
{
    public static void AddOpinionatedFrameworkPostgresUserTypes()
    {
        UserTypeMapper.AddUserType<Instant, InstantUserType>();
    }

    extension(IOpinionatedServiceCollection services)
    {
        public void AddNHibernateWithPostgres(Action<global::NHibernate.Cfg.Configuration> nHibernateConfig, Action<QueryExecutorOptions>? queryExecutorConfig = null)
        {
            var executorOptions = new QueryExecutorOptions();
            queryExecutorConfig?.Invoke(executorOptions);

            if (executorOptions.QueryExecutorConfiguration == null)
            {
                queryExecutorConfig += options => options.QueryExecutorConfiguration = new PostgresQueryExecutorConfiguration();
            }

            services.AddNHibernate(nHibernateConfig, queryExecutorConfig);
            AddOpinionatedFrameworkPostgresUserTypes();
        }
    }

    extension(FluentMappingsContainer container)
    {
        public void AddOpinionatedFrameworkPostgresMappings()
        {
            container.Add<EventMap>();
        }
    }
}