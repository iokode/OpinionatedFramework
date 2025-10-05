using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Tests.ResourceCommands;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.RestResourceControllers;

public class ResourceControllersTests : IClassFixture<ResourceControllersFixture>
{
    private readonly HttpClient Client;

    public ResourceControllersTests(ResourceControllersFixture fixture, ITestOutputHelper output)
    {
        fixture.TestOutputHelperFactory = () => output;
        Client = fixture.Client;
    }

    [Fact]
    public async Task GetUserById_Success()
    {
        // Arrange
        var id = 123;

        // Act
        var response = await Client.GetAsync($"/user/by-code/{id}");

        // Assert
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(id.ToString(), content);
    }

    [Fact]
    public async Task PostCreateUser_Success()
    {
        // Arrange
        var input = new CreateUserInput
        {
            Name = "Alice"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/user", input);

        // Assert
        var output = await response.Content.ReadFromJsonAsync<CreateUserOutput>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(output);
        Assert.Equal(input.Name, output.Id);
    }
}
