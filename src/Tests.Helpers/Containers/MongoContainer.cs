using System;
using System.Threading.Tasks;
using Docker.DotNet;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.Helpers.Containers;

public class MongoContainer : IAsyncLifetime
{
    private readonly DockerClient docker = DockerHelper.DockerClient;
    public string ContainerId = null!;

    public async Task InitializeAsync()
    {
        var mongoOptions = MongoHelper.DefaultOptions;

        await MongoHelper.PullMongoImage(docker);
        ContainerId = await MongoHelper.RunMongoContainer(docker);
        await MongoHelper.WaitUntilMongoServerIsReady(docker, ContainerId, mongoOptions);
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