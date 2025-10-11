using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres;
using IOKode.OpinionatedFramework.Logging;
using IOKode.OpinionatedFramework.Tests.Helpers;
using IOKode.OpinionatedFramework.Tests.Helpers.Containers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.ResourceControllers.Config;

public class ResourceControllersFixture : IAsyncLifetime
{
    public required HttpClient Client { get; set; }
    public Func<ITestOutputHelper> TestOutputHelperFactory { get; set; } = null!;
    public readonly PostgresContainer PostgresContainer = new();

    public async Task InitializeAsync()
    {
        await PostgresContainer.InitializeAsync();
        string connectionString = PostgresHelper.ConnectionString;
        Container.Services.AddTransient<ILogging>(_ => new XUnitLogging(TestOutputHelperFactory()));
        Container.Services.AddDefaultCommandExecutor(_ => {});
        Container.Services.AddNHibernateWithPostgres(cfg =>
        {
            Fluently.Configure(cfg)
                .Database(PostgreSQLConfiguration.PostgreSQL83
                    .ConnectionString(connectionString))
                .BuildConfiguration();
        });
        Container.Services.AddControllers().AddApplicationPart(typeof(ResourceControllersFixture).Assembly);
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services => services.AddControllers().AddApplicationPart(typeof(ResourceControllersFixture).Assembly))
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints => endpoints.MapControllers());
            });

        var server = new TestServer(webHostBuilder);
        Client = server.CreateClient();

        Container.Initialize();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await PostgresContainer.DisposeAsync();
    }
}