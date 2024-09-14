using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Tests.AspNetCoreIntegration.SessionEntryPoint;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.AspNetCoreIntegration;

public class SessionTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task SessionMiddleware_SerializesPipelineDataToSession()
    {
        // Arrange
        var client = factory.WithWebHostBuilder(_ => { }).CreateClient();

        // Act
        var response1 = await client.GetAsync("/endpoint1");
        var response2 = await client.GetAsync("/endpoint2");

        // Assert
        response1.EnsureSuccessStatusCode();
        response2.EnsureSuccessStatusCode();

        var sessionValue = client.DefaultRequestHeaders.GetValues("Set-Cookie");
        Assert.Contains(".Session", sessionValue);
    }
}