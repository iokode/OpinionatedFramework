using System.Threading.Tasks;
using Docker.DotNet;
using IOKode.OpinionatedFramework.TestHelpers.Configuration;
using Xunit;

namespace IOKode.OpinionatedFramework.TestHelpers.Containers;

public class PostgresContainer : IAsyncLifetime
{
    private readonly DockerClient docker = DockerHelper.DockerClient;
    public string ContainerId = null!;

    public async Task InitializeAsync()
    {
        var options = PostgresOptions.Default;
        await PostgresHelper.PullPostgresImage(docker, options);
        ContainerId = await PostgresHelper.RunPostgresContainer(docker, options);
        await PostgresHelper.WaitUntilPostgresServerIsReady(docker, ContainerId, options);
    }

    public async Task DisposeAsync()
    {
        try
        {
            await DockerHelper.RemoveContainer(docker, ContainerId);
            docker.Dispose();
        }
        catch (System.ObjectDisposedException){}
    }
}
