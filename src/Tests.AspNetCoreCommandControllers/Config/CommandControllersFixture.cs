using System;
using System.Net.Http;
using IOKode.OpinionatedFramework.AspNetCoreIntegrations.CommandControllers;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;
using IOKode.OpinionatedFramework.Logging;
using IOKode.OpinionatedFramework.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.AspNetCoreCommandControllers;

public class CommandControllersFixture : IDisposable
{
    public HttpClient HttpClient { get; }
    public Func<ITestOutputHelper> TestOutputHelperFactory { get; set; } = null!;

    public CommandControllersFixture()
    {
        Container.Services.AddTransient<ILogging>(_ => new XUnitLogging(TestOutputHelperFactory()));
        Container.Services.AddDefaultCommandExecutor(_ => {});
        Container.Services.AddCommandControllers();
        Container.Services.ScanForControllersCommands([typeof(TestCommand).Assembly]);
        Container.Services.ScanForControllersJsonConverters([typeof(TestCommand).Assembly]);
        
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services => services.AddCommandControllers())
            .Configure(cfg => cfg.UseCommandControllers());
        var server = new TestServer(webHostBuilder);
        HttpClient = server.CreateClient();
        Container.Initialize();
    }

    public void Dispose()
    {
        HttpClient.Dispose();
    }
}