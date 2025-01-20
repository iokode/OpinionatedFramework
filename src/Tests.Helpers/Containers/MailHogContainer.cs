using System;
using System.Threading.Tasks;
using Docker.DotNet;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.Helpers.Containers;

public class MailHogContainer : IAsyncLifetime
{
    private readonly DockerClient docker = DockerHelper.DockerClient;
    public string ContainerId = null!;

    public async Task InitializeAsync()
    {
        await MailHogHelper.PullMailHogImage(docker);
        ContainerId = await MailHogHelper.RunMailHogContainer(docker);
        await MailHogHelper.WaitUntilMailHogServerIsReady(docker, ContainerId);
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