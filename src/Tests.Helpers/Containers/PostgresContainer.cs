using System;
using System.Threading.Tasks;
using Docker.DotNet;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.Helpers.Containers;

public class PostgresContainer : IAsyncLifetime
{
    private readonly DockerClient docker = DockerHelper.DockerClient;
    public string ContainerId = null!;

    public async Task InitializeAsync()
    {
        var connectionString = PostgresHelper.ConnectionString;

        await PostgresHelper.PullPostgresImage(docker);
        ContainerId = await PostgresHelper.RunPostgresContainer(docker);
        await PostgresHelper.WaitUntilPostgresServerIsReady(docker, ContainerId, connectionString);
    }

    public async Task DisposeAsync()
    {
        try
        {
            await DockerHelper.RemoveContainer(docker, ContainerId);
            docker.Dispose();
        }
        catch (ObjectDisposedException){}
    }
}