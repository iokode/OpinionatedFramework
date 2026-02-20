using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using IOKode.OpinionatedFramework.AspNetCoreIntegrations.Middlewares;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;
using IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres;
using IOKode.OpinionatedFramework.Logging;
using IOKode.OpinionatedFramework.Tests.Helpers;
using IOKode.OpinionatedFramework.Tests.Helpers.Containers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        Container.Services.AddDefaultCommandExecutor(options =>
        {
            options.AddMiddleware<ResourceNotFoundCommandMiddleware>();
        });
        Container.Services.AddNHibernateWithPostgres(options =>
        {
            Fluently.Configure(options)
                .Database(PostgreSQLConfiguration.PostgreSQL83
                    .ConnectionString(connectionString))
                .BuildConfiguration();
        }, options =>
        {
           options.Middlewares.Add(new ResourceNotFoundQueryMiddleware());
        });
        Container.Services.AddControllers(options =>
        {
            options.Filters.Add(new ProducesAttribute("application/json"));
        }).AddApplicationPart(typeof(ResourceControllersFixture).Assembly);

        Container.Services.AddOpenApi("v1");
        var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddControllers(options =>
                        {
                            options.Filters.Add(new ProducesAttribute("application/json"));
                        }).AddApplicationPart(typeof(ResourceControllersFixture).Assembly);
                        services.AddOpenApi("v1");
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                            endpoints.MapOpenApi();
                        });
                    });
            })
            .StartAsync();

        Client = host.GetTestClient();

        Container.Initialize();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await PostgresContainer.DisposeAsync();
    }
}