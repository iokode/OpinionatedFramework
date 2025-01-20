using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Emailing;
using IOKode.OpinionatedFramework.Jobs.Extensions;
using IOKode.OpinionatedFramework.Tests.Hangfire.Config;
using IOKode.OpinionatedFramework.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.Hangfire;

[Collection(nameof(JobsTestsFixtureCollection))]
public class FoundationJobsTest(JobsTestsFixture fixture, ITestOutputHelper output) : JobsTestsBase(fixture, output)
{
    public static bool IsExecutedSendEmailJob;

    [Fact]
    public async Task SendEmailJob()
    {
        // Arrange
        var email = Email.CreateBuilder()
            .From("iokode@example.com")
            .To("iokode@example.net")
            .Subject("Email sending test.")
            .ToEmail();

        // Act
        await new SendEmailJobArguments(email).EnqueueAsync(default);
        await PollingUtility.WaitUntilTrueAsync(() => IsExecutedSendEmailJob, 20000, 1000);

        // Assert
        Assert.True(IsExecutedSendEmailJob);
    }
}