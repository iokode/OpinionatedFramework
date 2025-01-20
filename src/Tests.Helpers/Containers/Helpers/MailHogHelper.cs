using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Flurl.Http;
using IOKode.OpinionatedFramework.Utilities;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.Helpers.Containers;

public static class MailHogHelper
{
    public static async Task WaitUntilMailHogServerIsReady(DockerClient docker, string containerId, ITestOutputHelper? output = null)
    {
        bool mailServerIsReady = await PollingUtility.WaitUntilTrueAsync(async () =>
        {
            var containerInspect = await docker.Containers.InspectContainerAsync(containerId);
            bool containerIsReady = containerInspect.State.Running;
            if (!containerIsReady)
            {
                return false;
            }

            try
            {
                await "http://localhost:8025/api/v2/messages".GetJsonAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }, timeout: 30_000, pollingInterval: 1_000);

        if (!mailServerIsReady)
        {
            throw new TimeoutException("Failed to start MailHog server within the allowed time (30s).");
        }
    }

    public static async Task<string> RunMailHogContainer(DockerClient docker, ITestOutputHelper? output = null)
    {
        var container = await docker.Containers.CreateContainerAsync(new CreateContainerParameters()
        {
            Image = "mailhog/mailhog",
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {"1025/tcp", new[] {new PortBinding {HostPort = "1025"}}},
                    {"8025/tcp", new[] {new PortBinding {HostPort = "8025"}}},
                }
            },
            Name = "oftest_mailkit_mailhog"
        });

        var containerId = container.ID;
        await docker.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
        return containerId;
    }

    public static async Task PullMailHogImage(DockerClient docker, ITestOutputHelper? output = null)
    {
        await docker.Images.CreateImageAsync(new ImagesCreateParameters
        {
            FromImage = "mailhog/mailhog",
            Tag = "latest"
        }, null, new Progress<JSONMessage>(message => { output?.WriteLine(message.Status); }));
    }
}