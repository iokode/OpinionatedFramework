using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.AspNetCoreIntegrations;
using IOKode.OpinionatedFramework.Tests.ResourceControllers.Config;
using IOKode.OpinionatedFramework.Tests.ResourceControllers.Resources.Controllers;
using IOKode.OpinionatedFramework.Tests.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
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
    public void SourceCommandAttribForCommandIsApplied_Success()
    {
        // Arrange
        var controllerType = typeof(UserResourceController);
        var controllerMethod = controllerType.GetMethod(nameof(UserResourceController.RetrieveByCodeAsync))!;
        var sourceCommand = typeof(RetrieveUserByKeyCommand).ToString();
        var sourceCommandInController = controllerMethod.GetCustomAttribute<SourceCommandAttribute>()!.CommandTypeString;

        // Assert
        Assert.Equal(sourceCommand, sourceCommandInController);
    }

    [Fact]
    public void SourceCommandAttribForQueryIsApplied_Success()
    {
        // Arrange
        var controllerType = typeof(UserResourceController);
        var controllerMethod = controllerType.GetMethod(nameof(UserResourceController.RetrieveNameByUserNameAsync))!;
        var sourceQuery = typeof(RetrieveUserSubResourceByResourceKeyQuery).ToString();
        var sourceQueryInController = controllerMethod.GetCustomAttribute<SourceCommandAttribute>()!.CommandTypeString;

        // Assert
        Assert.Equal(sourceQuery, sourceQueryInController);
    }
    
    [Fact]
    public async Task RetrieveUser_ByKey_Success()
    {
        // Arrange
        int id = 123;

        // Act
        var response = await this.client.GetAsync($"/users/code-{id}");

        // Assert
        var content = await response.Content.ReadFromJsonAsync<int>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(id, content);
    }
    
    [Fact]
    public async Task RetrieveUser_SubResourceParameterless_Success()
    {
        // Act
        var response = await this.client.GetAsync("/users/single");

        // Assert
        var content = await response.Content.ReadFromJsonAsync<string>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("single-user", content);
    }

    [Fact]
    public async Task RetrieveUser_SubResourceByResourceKey_Success()
    {
        // Arrange
        var name = "name";

        // Act
        var response = await this.client.GetAsync($"/users/name-{name}/name");

        // Assert
        var content = await response.Content.ReadFromJsonAsync<RetrieveUserSubResourceByResourceKeyQueryResult>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(name, content!.Name);
    }

    [Fact]
    public async Task RetrieveOrder_FromThisAssembly_Success()
    {
        // Arrange
        int id = 753;

        // Act
        var response = await this.client.GetAsync($"/orders/id-{id}");

        // Assert
        var result = await response.Content.ReadFromJsonAsync<string>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"order-{id}", result);
    }

    [Fact]
    public async Task ListUsers_FromQuery_Success()
    {
        // Act
        var response = await this.client.GetAsync("/users");

        // Assert
        var content = await response.Content.ReadFromJsonAsync<ListUsersQueryResult[]>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["user1", "user2", "user3"], content?.Select(result => result.Name) ?? []);
    }

    [Fact]
    public async Task ListUsers_SubresourceBySubresourceKeys_Success()
    {
        // Act
        var response = await this.client.GetAsync($"/users/by-key/key1-k1/key2-1");

        // Assert
        var content = await response.Content.ReadFromJsonAsync<string[]>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new []{"k1", "1"}, content);
    }
    
    [Fact]
    public async Task ListUsers_WithQueryParameters_Success()
    {
        // Act
        var response = await this.client.GetAsync("/users/by-key/key1-k1/key2-1?param2=true&param1=-1");

        // Assert
        var content = await response.Content.ReadFromJsonAsync<string[]>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new []{"k1", "1", "-1", "True"}, content);
    }

    [Fact]
    public async Task ListUsers_DeepSubresourceByKeyOnEachLevel_Success()
    {
        // Act
        var response = await this.client.GetAsync("/users/id-5/actives/is-active-true/related-users/key-3");

        // Assert
        var content = await response.Content.ReadFromJsonAsync<int[]>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new[] {5, 1, 3, 0}, content);
    }
    
    [Fact]
    public async Task ListUsers_WithQueryParametersForWrapperObject_Success()
    {
        // Act
        var response = await this.client.GetAsync("/users/id-5/actives/is-active-true/related-users/key-3?isSingle=true");

        // Assert
        var content = await response.Content.ReadFromJsonAsync<int[]>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new[] {5, 1, 3, 1}, content);
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
    public async Task UpdateUser_ByKey_Success()
    {
        // Arrange
        int id = 11;

        // Act
        var response = await this.client.PatchAsync($"/users/code-{id}", JsonContent.Create(new
        {
            Updated = true
        }));

        // Assert
        var content = await response.Content.ReadFromJsonAsync<string>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"updated-{id}-True", content);
    }

    [Fact]
    public async Task ReplaceUser_ByKey_Success()
    {
        // Arrange
        int id = 21;

        // Act
        var response = await this.client.PutAsync($"/users/code-{id}", new StringContent(string.Empty, Encoding.UTF8, "application/json"));

        // Assert
        var content = await response.Content.ReadFromJsonAsync<string>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"replaced-{id}", content);
    }

    [Fact]
    public async Task DeleteUser_ByCode_Success()
    {
        // Arrange
        int id = 31;

        // Act
        var response = await this.client.DeleteAsync($"/users/code-{id}");

        // Assert
        var content = await response.Content.ReadFromJsonAsync<string>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"deleted-{id}", content);
    }

    [Fact]
    public async Task DeleteUser_WithBody_Success()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, "/users")
        {
            Content = JsonContent.Create(new[] { 1, 2, 3 })
        };

        // Act
        var response = await this.client.SendAsync(request);

        // Assert
        var content = await response.Content.ReadFromJsonAsync<string>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("deleted-1,2,3", content);
    }

    [Fact]
    public async Task ActionUser_EnableByKeyWithBodyOnMultipleParameters_Success()
    {
        // Assert
        var code = "41";

        // Act
        var response = await this.client.PatchAsync($"/users/code-{code}/enable", JsonContent.Create(new
        {
            Enable = true,
            EnableName = "name",
        }));

        // Assert
        var content = await response.Content.ReadFromJsonAsync<string>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"enabled-by-code-{code}-name-True", content);
    }

    [Fact]
    public async Task ActionUser_RenameByKey_Success()
    {
        // Arrange
        var name = "Angel";

        // Act
        var response = await this.client.PatchAsync($"/users/name-{name}/rename", JsonContent.Create("Miguel"));

        // Assert
        var content = await response.Content.ReadFromJsonAsync<string>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Angel->Miguel", content);
    }

    [Fact]
    public async Task OpenApi_Success()
    {
        // Act
        var response = await this.client.GetAsync("/openapi/v1.json");

        // Assert
        var stream = await response.Content.ReadAsStreamAsync();
        var document = (await OpenApiDocument.LoadAsync(stream)).Document!;
        var responses = document.Paths["/orders/do-action"].Operations![HttpMethod.Patch].Responses!;

        Assert.Equal(3, responses.Count);

        var content204 = responses["204"].Content!;
        var content404 = responses["404"].Content!;
        var content500 = responses["500"].Content!;
        Assert.NotNull(responses["204"]);
        Assert.Null(content204);

        Assert.Single(content404);
        Assert.True(content404.ContainsKey("application/json"));

        Assert.Single(content500);
        Assert.True(content500.ContainsKey("application/json"));
    }

    [Fact]
    public async Task ResourceNotFound_Command_Returns404()
    {
        // Arrange
        int id = 123;

        // Act
        var response = await this.client.GetAsync($"/not-found-commands/id/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task ResourceNotFound_Query_Returns404()
    {
        // Arrange
        int id = 123;

        // Act
        var response = await this.client.GetAsync($"/not-found-queries/id/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}