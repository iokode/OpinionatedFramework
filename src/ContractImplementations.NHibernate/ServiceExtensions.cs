using System;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Persistence.Queries;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate;

public static class ServiceExtensions
{
    public static void AddNHibernate(this IOpinionatedServiceCollection services, Action<global::NHibernate.Cfg.Configuration> configuration)
    {
        AddNHibernate(services, configuration, new DefaultQueryExecutorConfiguration());
    }

    public static void AddNHibernate(this IOpinionatedServiceCollection services, Action<global::NHibernate.Cfg.Configuration> configuration, IQueryExecutorConfiguration queryExecutorConfiguration)
    {
        var config = new global::NHibernate.Cfg.Configuration();
        configuration(config);
        var sessionFactory = config.BuildSessionFactory();

        services.AddTransient<IUnitOfWorkFactory>(_ => new UnitOfWorkFactory(sessionFactory));
        services.AddTransient<IQueryExecutor>(_ => new QueryExecutor(sessionFactory, queryExecutorConfiguration));
    }
}