using Docker.DotNet;
using Docker.DotNet.Models;
using Flurl.Http;
using IOKode.OpinionatedFramework.Foundation.Emailing;
using MailKit.Security;
using Xunit;

namespace IOKode.OpinionatedFramework.ContractImplementations.MailKit.Tests;

public class MailKitTests : IAsyncLifetime
{
    private string _containerId = null!;
    private DockerClient _docker = null!;

    public async Task InitializeAsync()
    {
        _docker = new DockerClientConfiguration().CreateClient();
        await _docker.Images.CreateImageAsync(new ImagesCreateParameters
        {
            FromImage = "adamculp/mailslurper",
            Tag = "latest"
        }, null, null);

        var container = await _docker.Containers.CreateContainerAsync(new CreateContainerParameters()
        {
            Image = "adamculp/mailslurper",
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    { "2500/tcp", new[] { new PortBinding { HostPort = "2500" } } },
                    { "8080/tcp", new[] { new PortBinding { HostPort = "8080" } } },
                    { "8085/tcp", new[] { new PortBinding { HostPort = "8085" } } }
                }
            },
            Name = "OFTest/MailKit MailSlurper"
        });

        _containerId = container.ID;
        await _docker.Containers.StartContainerAsync(_containerId, new ContainerStartParameters());
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
        var mailKitOptions = new MailKitOptions
        {
            Host = "localhost",
            Port = 2500,
            Username = "",
            Password = "",
            Secure = SecureSocketOptions.Auto
        };

        var sender = new MailKitEmailSender(mailKitOptions);

        var email = Email.CreateBuilder()
            .From("from@example.com")
            .To("to@example.com")
            .Subject("Test email")
            .TextContent("This is a test email.")
            .ToEmail();

        await sender.SendAsync(email, default);
        await Task.Delay(5000);

        var receivedEmails = await "http://localhost:8085/api/v1/mail".GetJsonAsync();

        // Assert
        Assert.NotEmpty(receivedEmails.items);
        var lastReceivedEmail = receivedEmails.items.Last();

        Assert.Equal(email.From.ToString(), lastReceivedEmail.fromAddress);
        Assert.Equal(email.To.First().ToString(), lastReceivedEmail.toAddresses[0].address);
        Assert.Equal(email.Subject, lastReceivedEmail.subject);
        Assert.Equal(email.TextContent, lastReceivedEmail.body);
    }
}