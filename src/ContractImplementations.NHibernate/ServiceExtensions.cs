using System;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate;

public static class ServiceExtensions
{
    public static void AddNHibernate(this IOpinionatedServiceCollection services, Action<global::NHibernate.Cfg.Configuration> configuration)
    {
        var config = new global::NHibernate.Cfg.Configuration();
        configuration(config);
        var sessionfactory = config.BuildSessionFactory();

        services.AddTransient<IUnitOfWorkFactory>(_ => new UnitOfWorkFactory(sessionfactory));
    }
}