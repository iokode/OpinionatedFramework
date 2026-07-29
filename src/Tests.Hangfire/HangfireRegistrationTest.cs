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
    /// Every entry of the <c>Servers</c> dictionary runs as its own server, with its own queue and worker pool.
    /// </summary>
    /// <remarks>
    /// The assertions read the servers Hangfire itself announced in storage, so they describe what is actually
    /// processing jobs rather than the options objects the bootstrap composed. The <c>events</c> entry
    /// configures 9 workers and a named <c>ConfigureServer</c> delegate overrides it, while <c>default</c> has
    /// no delegate, so the two worker counts together pin down both that a named delegate reaches its own
    /// server and that it leaves the others alone.
    /// </remarks>
    [Fact]
    public async Task EachConfiguredServerRunsWithItsOwnOptions()
    {
        var monitoringApi = Locator.Resolve<JobStorage>().GetMonitoringApi();
        await PollingUtility.WaitUntilTrueAsync(() => monitoringApi.Servers().Count >= 2, 10000, 500);

        // Hangfire makes the server id globally unique by appending the process id and a guid to the name.
        var servers = monitoringApi.Servers().ToDictionary(server => server.Name.Split(':')[0]);

        Assert.Equal(["default", "events"], servers.Keys.Order());
        Assert.Equal(["default"], servers["default"].Queues);
        Assert.Equal(["events"], servers["events"].Queues);
        Assert.Equal(JobsTestsFixture.DefaultServerWorkerCount, servers["default"].WorkersCount);
        Assert.Equal(JobsTestsFixture.EventsServerWorkerCount, servers["events"].WorkersCount);
    }
}
