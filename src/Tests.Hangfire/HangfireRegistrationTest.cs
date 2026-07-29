using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Dashboard;
using IOKode.OpinionatedFramework.ServiceLocation;
using IOKode.OpinionatedFramework.Tests.Hangfire.Config;
using IOKode.OpinionatedFramework.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.Hangfire;

/// <summary>
/// Covers what the bootstrap leaves behind: a fully registered Hangfire and a server carrying the configured
/// options.
/// </summary>
[Collection(nameof(JobsTestsFixtureCollection))]
public class HangfireRegistrationTest(JobsTestsFixture fixture, ITestOutputHelper output)
    : JobsTestsBase(fixture, output)
{
    /// <summary>
    /// An application resolves Hangfire from the container without registering any of it itself.
    /// </summary>
    /// <remarks>
    /// <see cref="IGlobalConfiguration"/> and <see cref="RouteCollection"/> are what
    /// <c>UseHangfireDashboard</c> resolves, so an application that selects the driver can mount the dashboard.
    /// </remarks>
    [Fact]
    public void ContainerResolvesTheHangfireSurface()
    {
        Assert.NotNull(Locator.Resolve<IGlobalConfiguration>());
        Assert.NotNull(Locator.Resolve<JobStorage>());
        Assert.NotNull(Locator.Resolve<RouteCollection>());
        Assert.NotNull(Locator.Resolve<IBackgroundJobClient>());
        Assert.NotNull(Locator.Resolve<IRecurringJobManager>());
    }

    /// <summary>
    /// The running server carries the queues read from configuration and the worker count set by the verb.
    /// </summary>
    /// <remarks>
    /// The assertions read the server Hangfire itself announced in storage, so they describe the server that is
    /// actually processing jobs rather than the options object the bootstrap composed. Configuration sets
    /// <c>WorkerCount</c> to 4 and the verb overrides it, while <c>Queues</c> is left to configuration, so the
    /// two assertions together pin down the precedence rule in both directions.
    /// </remarks>
    [Fact]
    public async Task ServerRunsWithTheConfiguredOptions()
    {
        var monitoringApi = Locator.Resolve<JobStorage>().GetMonitoringApi();
        await PollingUtility.WaitUntilTrueAsync(() => monitoringApi.Servers().Count > 0, 10000, 500);

        var server = Assert.Single(monitoringApi.Servers());
        Assert.Equal(JobsTestsFixture.ConfiguredWorkerCount, server.WorkersCount);
        Assert.Equal(["default", "events"], server.Queues.Order());
    }
}
