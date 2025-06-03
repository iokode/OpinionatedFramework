using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.AspNetCoreCommandControllers;

public class SingleControllerTests : IClassFixture<CommandControllersFixture>
{
    private readonly HttpClient httpClient;
    private readonly ITestOutputHelper output;

    public SingleControllerTests(CommandControllersFixture fixture, ITestOutputHelper output)
    {
        this.output = output;
        fixture.TestOutputHelperFactory = () => output;
        httpClient = fixture.HttpClient;
    }


    [Fact]
    public async Task InvokeCommand_WithValidNonGenericCommand_ReturnsOk()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/api")
        {
            Headers = { {"X-Command", nameof(TestCommand)} },
        };

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task InvokeCommand_WithValidGenericCommand_ReturnsOk()
    {
        // Arrange
        var requestBody = """{ "id": 42 }""";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        
        var request = new HttpRequestMessage(HttpMethod.Post, "/api")
        {
            Headers = { {"X-Command", nameof(Test2Command)} },
            Content = content
        };

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        output.WriteLine($"Response: {responseContent}");
    }

    [Fact]
    public async Task InvokeCommand_WithoutCommandHeader_ReturnsBadRequest()
    {
        // Arrange
        var requestBody = """{ "id": 42 }""";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // Act
        var response = await httpClient.PostAsync("/api", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InvokeCommand_WithWrongHttpMethod_ReturnsMethodNotAllowed()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api");
        request.Headers.Add("X-Command", nameof(TestCommand));

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task InvokeCommand_WithUnknownCommand_ReturnsBadRequest()
    {
        // Arrange
        var requestBody = """{ "id": 42 }""";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        
        var request = new HttpRequestMessage(HttpMethod.Post, "/api")
        {
            Content = content
        };
        request.Headers.Add("X-Command", "UnknownCommand");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InvokeCommand_WithInvalidJson_ReturnsBadRequest()
    {
        // Arrange
        var requestBody = "invalid json";
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        
        var request = new HttpRequestMessage(HttpMethod.Post, "/api")
        {
            Content = content
        };
        request.Headers.Add("X-Command", nameof(TestCommand));

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

}