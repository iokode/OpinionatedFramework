using System;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.QueryExecutor;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.UnitOfWork;
using IOKode.OpinionatedFramework.Persistence.Queries;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate;

public static class ServiceExtensions
{
    extension(IOpinionatedServiceCollection services)
    {
        public void AddNHibernate(Action<global::NHibernate.Cfg.Configuration> nHibernateConfig, Action<QueryExecutorOptions>? queryExecutorConfig = null)
        {
            var nhibernateConfiguration = new global::NHibernate.Cfg.Configuration();
            nHibernateConfig.Invoke(nhibernateConfiguration);

            var executorOptions = new QueryExecutorOptions();
            queryExecutorConfig?.Invoke(executorOptions);

            var sessionFactory = nhibernateConfiguration.BuildSessionFactory();

            services.AddTransient<IUnitOfWorkFactory>(_ => new UnitOfWorkFactory(sessionFactory));
            services.AddTransient<IQueryExecutorFactory>(_ => 
                new QueryExecutorFactory(sessionFactory, executorOptions.QueryExecutorConfiguration ?? new QueryExecutorDefaultConfiguration()));
            services.AddTransient<IQueryExecutor>(sp =>
                sp.GetRequiredService<IQueryExecutorFactory>().Create(executorOptions.Middlewares.ToArray()));
        }
    }
}