using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Flurl.Http;
using IOKode.OpinionatedFramework.ContractImplementations.MailKit;
using IOKode.OpinionatedFramework.Emailing;
using IOKode.OpinionatedFramework.Utilities;
using MailKit.Security;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.MailKit;

public class MailKitTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private string _containerId = null!;
    private DockerClient _docker = null!;

    private MailKitOptions _mailKitOptions = new()
    {
        Host = "localhost",
        Port = 1025,
        Authenticate = false,
        Secure = SecureSocketOptions.Auto
    };

    public MailKitTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        async Task waitUntilMailHogServerIsReady()
        {
            bool mailServerIsReady = await PollingUtility.WaitUntilTrueAsync(async () =>
            {
                var containerInspect = await _docker.Containers.InspectContainerAsync(_containerId);
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
                _output.WriteLine("Failed to start MailHog server within the allowed time (30s).");
            }
        }

        async Task runMailHogContainer()
        {
            var container = await _docker.Containers.CreateContainerAsync(new CreateContainerParameters()
            {
                Image = "mailhog/mailhog",
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        { "1025/tcp", new[] { new PortBinding { HostPort = "1025" } } },
                        { "8025/tcp", new[] { new PortBinding { HostPort = "8025" } } },
                    }
                },
                Name = "oftest_mailkit_mailhog"
            });

            _containerId = container.ID;
            await _docker.Containers.StartContainerAsync(_containerId, new ContainerStartParameters());
        }

        async Task pullMailHogImage()
        {
            await _docker.Images.CreateImageAsync(new ImagesCreateParameters
            {
                FromImage = "mailhog/mailhog",
                Tag = "latest"
            }, null, new Progress<JSONMessage>(message => { _output.WriteLine(message.Status); }));
        }

        _docker = new DockerClientConfiguration().CreateClient();
        await pullMailHogImage();
        await runMailHogContainer();
        await waitUntilMailHogServerIsReady();
    }

    public async Task DisposeAsync()
    {
        await _docker.Containers.StopContainerAsync(_containerId, new ContainerStopParameters());
        await _docker.Containers.RemoveContainerAsync(_containerId, new ContainerRemoveParameters());
        _docker.Dispose();
    }

    [Fact]
    public async Task SendEmail_WithValidConfiguration_SendsSuccessfully()
    {
        var sender = new MailKitEmailSender(_mailKitOptions);

        var email = Email.CreateBuilder()
            .From("from@example.com")
            .To("to@example.com")
            .Subject("Test email")
            .TextContent("This is a test email.")
            .ToEmail();

        await sender.SendAsync(email, default);

        bool didEmailReachTheServer = await PollingUtility.WaitUntilTrueAsync(async () =>
        {
            var receivedEmails = await "http://localhost:8025/api/v2/messages".GetJsonAsync<MailHogEmailResponse>();
            return receivedEmails.Count > 0;
        }, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));

        if (!didEmailReachTheServer)
        {
            Assert.Fail("The email never reached the server.");
        }

        var receivedEmails = await "http://localhost:8025/api/v2/messages".GetJsonAsync<MailHogEmailResponse>();

        // Assert
        Assert.Single(receivedEmails.Items);
        var receivedEmail = receivedEmails.Items[0];

        Assert.Equal(email.From.ToString(), receivedEmail.Raw.From);
        Assert.Equal(email.To.First().ToString(), receivedEmail.Raw.To[0]);
        Assert.Equal(email.Subject, receivedEmail.Content.Headers["Subject"][0]);
        Assert.Equal(email.TextContent, receivedEmail.Content.Body);
        Assert.Equal(email.MessageId.ToString(), receivedEmail.Content.Headers["Message-Id"][0]);
    }
}