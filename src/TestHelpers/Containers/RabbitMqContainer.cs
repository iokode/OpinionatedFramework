using System.Threading.Tasks;
using Docker.DotNet;
using IOKode.OpinionatedFramework.TestHelpers.Configuration;
using Xunit;

namespace IOKode.OpinionatedFramework.TestHelpers.Containers;

public class RabbitMqContainer : IAsyncLifetime
{
    private readonly DockerClient docker = DockerHelper.DockerClient;
    public string ContainerId = null!;
    public RabbitMqOptions Options { get; } = RabbitMqOptions.Default;

    public async Task InitializeAsync()
    {
        await RabbitMqHelper.PullRabbitMqImage(docker, Options);
        ContainerId = await RabbitMqHelper.RunRabbitMqContainer(docker, Options);
        await RabbitMqHelper.WaitUntilRabbitMqServerIsReady(docker, ContainerId, Options);
    }

    public async Task DisposeAsync()
    {
        try
        {
            // The client is a process-wide singleton shared by every container helper, so it is not disposed
            // here: doing so would break any fixture that starts a container later in the same test run.
            await DockerHelper.RemoveContainer(docker, ContainerId);
        }
        catch (System.ObjectDisposedException) { }
    }
}
