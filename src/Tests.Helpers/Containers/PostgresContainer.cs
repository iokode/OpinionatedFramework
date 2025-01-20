using System;
using System.Threading.Tasks;
using Docker.DotNet;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.Helpers.Containers;

public class PostgresContainer(DockerClient docker, ITestOutputHelper? output = null) : IAsyncLifetime
{
    public string ContainerId = null!;
    private readonly ITestOutputHelper? output = output;

    public PostgresContainer() : this(DockerHelper.DockerClient)
    {
    }

    public async Task InitializeAsync()
    {
        var connectionString = PostgresHelper.DefaultConnectionString;

        await PostgresHelper.PullPostgresImage(docker, output);
        ContainerId = await PostgresHelper.RunPostgresContainer(docker, output);
        await PostgresHelper.WaitUntilPostgresServerIsReady(docker, ContainerId, connectionString, output);
    }

    public async Task DisposeAsync()
    {
        try
        {
            await DockerHelper.RemoveContainer(docker, ContainerId);
        }
        catch (ObjectDisposedException){}
    }
}