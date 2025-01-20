using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace IOKode.OpinionatedFramework.Tests.Helpers.Containers;

public static class DockerHelper
{
    public static async Task RemoveContainer(DockerClient docker, string containerId)
    {
        await docker.Containers.StopContainerAsync(containerId, new ContainerStopParameters());
        await docker.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters());
    }

    private static DockerClient? dockerClient = null!;
    public static DockerClient DockerClient => dockerClient ??= new DockerClientConfiguration().CreateClient();
}