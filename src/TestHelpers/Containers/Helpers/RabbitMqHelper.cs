using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using IOKode.OpinionatedFramework.TestHelpers.Configuration;
using IOKode.OpinionatedFramework.Utilities;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.TestHelpers.Containers;

public static class RabbitMqHelper
{
    public static async Task WaitUntilRabbitMqServerIsReady(DockerClient docker, string containerId,
        RabbitMqOptions options, ITestOutputHelper? output = null)
    {
        bool serverIsReady = await PollingUtility.WaitUntilTrueAsync(async () =>
        {
            output?.WriteLine("Waiting for RabbitMQ server to be ready...");
            var containerInspect = await docker.Containers.InspectContainerAsync(containerId);
            if (!containerInspect.State.Running)
            {
                output?.WriteLine("Not ready yet...");
                return false;
            }

            try
            {
                // The AMQP port starts listening before the broker can serve, and a connection attempted in
                // that window fails the handshake, so the port alone is not a readiness signal. The broker
                // announces the real thing in its log.
                using var client = new TcpClient();
                await client.ConnectAsync("localhost", int.Parse(options.HostPort));
                if (!client.Connected)
                {
                    return false;
                }

                return await LogContainsStartupCompleteAsync(docker, containerId);
            }
            catch (Exception ex)
            {
                output?.WriteLine("Not ready yet...");
                output?.WriteLine(ex.Message);
                return false;
            }
        }, timeout: 90_000, pollingInterval: 1_000);

        if (!serverIsReady)
        {
            throw new TimeoutException("Failed to start RabbitMQ server within the allowed time (90s).");
        }
    }

    public static async Task<string> RunRabbitMqContainer(DockerClient docker, RabbitMqOptions options)
    {
        var container = await docker.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = options.ImageWithTag,
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {"5672/tcp", [new PortBinding {HostPort = options.HostPort}]},
                }
            },
            Env =
            [
                $"RABBITMQ_DEFAULT_USER={options.Username}",
                $"RABBITMQ_DEFAULT_PASS={options.Password}"
            ],
            Name = options.ContainerName
        });

        await docker.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
        return container.ID;
    }

    public static async Task PullRabbitMqImage(DockerClient docker, RabbitMqOptions options,
        ITestOutputHelper? output = null)
    {
        await docker.Images.CreateImageAsync(new ImagesCreateParameters
        {
            FromImage = options.Image,
            Tag = options.Tag
        }, null, new Progress<JSONMessage>(message => { output?.WriteLine(message.Status); }));
    }

    private static async Task<bool> LogContainsStartupCompleteAsync(DockerClient docker, string containerId)
    {
        using var logs = await docker.Containers.GetContainerLogsAsync(containerId, tty: false,
            new ContainerLogsParameters {ShowStdout = true, ShowStderr = true, Tail = "300"});

        var (stdout, stderr) = await logs.ReadOutputToEndAsync(default);
        return stdout.Contains("Server startup complete", StringComparison.Ordinal)
               || stderr.Contains("Server startup complete", StringComparison.Ordinal);
    }
}
