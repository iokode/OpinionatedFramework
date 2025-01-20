using System;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using IOKode.OpinionatedFramework.ContractImplementations.MailKit;
using IOKode.OpinionatedFramework.Emailing;
using IOKode.OpinionatedFramework.Tests.Helpers.Containers;
using IOKode.OpinionatedFramework.Utilities;
using MailKit.Security;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.MailKit;

public class MailKitTests : IClassFixture<MailHogContainer>
{
    private readonly MailKitOptions mailKitOptions = new()
    {
        Host = "localhost",
        Port = 1025,
        Authenticate = false,
        Secure = SecureSocketOptions.Auto
    };

    [Fact]
    public async Task SendEmail_WithValidConfiguration_SendsSuccessfully()
    {
        var sender = new MailKitEmailSender(mailKitOptions);

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