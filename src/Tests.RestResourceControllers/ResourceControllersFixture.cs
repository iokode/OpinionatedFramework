using System;
using System.Net.Http;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;
using IOKode.OpinionatedFramework.Logging;
using IOKode.OpinionatedFramework.Tests.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.RestResourceControllers;

public class ResourceControllersFixture : IDisposable
{
    public HttpClient Client { get; }
    public Func<ITestOutputHelper> TestOutputHelperFactory { get; set; } = null!;

    public ResourceControllersFixture()
    {
        Container.Services.AddTransient<ILogging>(_ => new XUnitLogging(TestOutputHelperFactory()));
        Container.Services.AddDefaultCommandExecutor(_ => {});
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

    public void Dispose()
    {
        Client.Dispose();
    }
}