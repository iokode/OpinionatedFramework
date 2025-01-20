using System;
using System.Threading.Tasks;
using Docker.DotNet;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.Helpers.Containers;

public class MongoContainer(DockerClient docker, ITestOutputHelper? output = null) : IAsyncLifetime
{
    public string ContainerId = null!;
    private DockerClient docker => DockerHelper.DockerClient;
    
    public MongoContainer() : this(DockerHelper.DockerClient)
    {
    }
    
    public async Task InitializeAsync()
    {
        var mongoOptions = MongoHelper.DefaultOptions;
        ContainerId = await MongoHelper.PullRunAndWaitMongoContainerAsync(docker, mongoOptions);
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