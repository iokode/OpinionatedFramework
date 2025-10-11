using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Tests.ResourceControllers.Config;
using IOKode.OpinionatedFramework.Tests.Resources;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.ResourceControllers;

public class ResourceControllersTests : IClassFixture<ResourceControllersFixture>
{
    private readonly HttpClient client;

    public ResourceControllersTests(ResourceControllersFixture fixture, ITestOutputHelper output)
    {
        fixture.TestOutputHelperFactory = () => output;
        client = fixture.Client;
    }

    [Fact]
    public async Task RetrieveUser_Success()
    {
        // Arrange
        int id = 321;

        // Act
        var response = await this.client.GetAsync($"/users/{id}");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(id.ToString(), content);
    }

    [Fact]
    public async Task RetrieveUser_ByKey_Success()
    {
        // Arrange
        int id = 123;

        // Act
        var response = await this.client.GetAsync($"/users/by-code/{id}");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(id.ToString(), content);
    }
    
    [Fact]
    public async Task RetrieveUser_Single_Success()
    {
        // Act
        var response = await this.client.GetAsync("/user");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("single-user", content);
    }

    [Fact]
    public async Task RetrieveUser_SingleFromSqlQuery_Success()
    {
        // Act
        var response = await this.client.GetAsync("/user/by-global");

        // Assert
        var result = await response.Content.ReadFromJsonAsync<GetSingleUserQueryResult>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("global-user", result?.Name ?? string.Empty);
    }

    [Fact]
    public async Task RetrieveUser_ByKeyFromSqlQuery_Success()
    {
        // Arrange
        var name = "Angel";

        // Act
        var response = await this.client.GetAsync($"/users/by-name/{name}");

        // Assert
        var result = await response.Content.ReadFromJsonAsync<GetUserByNameQueryResult>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(name, result?.Name ?? string.Empty);
    }

    [Fact]
    public async Task RetrieveOrder_FromThisAssembly_Success()
    {
        // Arrange
        int id = 753;

        // Act
        var response = await this.client.GetAsync($"/orders/{id}");

        // Assert
        var result = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"order-{id}", result);
    }

    [Fact]
    public async Task ListUsers_Success()
    {
        // Act
        var response = await this.client.GetAsync("/users");

        // Assert
        var content = await response.Content.ReadFromJsonAsync<string[]>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new[] {"user1", "user2", "user3"}, content);
    }

    [Fact]
    public async Task ListUsers_ByKey_Success()
    {
        // Act
        var response = await this.client.GetAsync("/users/by-actives");

        // Assert
        var content = await response.Content.ReadFromJsonAsync<int[]>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new[] {1, 2, 3}, content);
    }
    
    [Fact]
    public async Task ListUsers_ByKeyAndFilters_Success()
    {
        // Act
        var response = await this.client.GetAsync("/users/by-actives?isSingle=true");

        // Assert
        var content = await response.Content.ReadFromJsonAsync<int[]>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new[] {1}, content);
    }
    
    [Fact]
    public async Task ListProducts_FromQuery_Success()
    {
        // Act
        var response = await this.client.GetAsync("/products");

        // Assert
        var content = await response.Content.ReadFromJsonAsync<ListProductsQueryResult[]>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["product1", "product2", "product3"], content?.Select(result => result.Name) ?? []);;
    }

    [Fact]
    public async Task CreateUser_Success()
    {
        // Arrange
        var input = new CreateUserInput
        {
            Name = "Angel"
        };

        // Act
        var response = await this.client.PostAsJsonAsync("/users", input);

        // Assert
        var output = await response.Content.ReadFromJsonAsync<CreateUserOutput>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(output);
        Assert.Equal(input.Name, output.Id);
    }

    [Fact]
    public async Task UpdateUser_Success()
    {
        // Arrange
        int id = 10;

        // Act
        var response = await this.client.PatchAsync($"/users/{id}", new StringContent(string.Empty, Encoding.UTF8, "application/json"));

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"updated-{id}", content);
    }

    [Fact]
    public async Task UpdateUser_ByKey_Success()
    {
        // Arrange
        int id = 11;

        // Act
        var response = await this.client.PatchAsync($"/users/by-code/{id}", new StringContent(string.Empty, Encoding.UTF8, "application/json"));

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"updated-{id}", content);
    }

    [Fact]
    public async Task ReplaceUser_Success()
    {
        // Arrange
        int id = 20;
        var response = await this.client.PutAsync($"/users/{id}", new StringContent(string.Empty, Encoding.UTF8, "application/json"));

        // Act
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"replaced-{id}", content);
    }

    [Fact]
    public async Task ReplaceUser_ByKey_Success()
    {
        // Arrange
        int id = 21;

        // Act
        var response = await this.client.PutAsync($"/users/by-code/{id}", new StringContent(string.Empty, Encoding.UTF8, "application/json"));

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"replaced-{id}", content);
    }

    [Fact]
    public async Task DeleteUser_Success()
    {
        // Arrange
        int id = 30;

        // Act
        var response = await this.client.DeleteAsync($"/users/{id}");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"deleted-{id}", content);
    }

    [Fact]
    public async Task DeleteUser_ByCode_Success()
    {
        // Arrange
        int id = 31;

        // Act
        var response = await this.client.DeleteAsync($"/users/by-code/{id}");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"deleted-{id}", content);
    }

    [Fact]
    public async Task ActionEnableUser_Success()
    {
        // Arrange
        int id = 40;

        // Act
        var response = await this.client.PatchAsync($"/users/{id}/enable", new StringContent(string.Empty, Encoding.UTF8, "application/json"));

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"enabled-{id}", content);
    }

    [Fact]
    public async Task ActionEnableUser_ByKey_Success()
    {
        // Assert
        var code = "41";

        // Act
        var response = await this.client.PatchAsync($"/users/by-code/{code}/enable", new StringContent(string.Empty, Encoding.UTF8, "application/json"));

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"enabled-by-code-{code}", content);
    }
}