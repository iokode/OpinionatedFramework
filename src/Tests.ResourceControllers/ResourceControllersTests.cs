using System.Collections.Generic;
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
using Microsoft.OpenApi;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.ResourceControllers;

public class ResourceControllersTests : IClassFixture<ResourceControllersFixture>
{
    private readonly HttpClient client;
    private readonly HttpClient clientWithoutModelStateInvalidFilter;

    public ResourceControllersTests(ResourceControllersFixture fixture, ITestOutputHelper output)
    {
        fixture.TestOutputHelperFactory = () => output;
        client = fixture.Client;
        clientWithoutModelStateInvalidFilter = fixture.ClientWithoutModelStateInvalidFilter;
    }

    [Fact]
    public void SourceCommandAttribForCommandIsApplied_Success()
    {
        // Arrange
        var controllerType = typeof(UserResourceController);
        var controllerMethod = controllerType.GetMethod(nameof(UserResourceController.RetrieveByCodeAsync))!;
        var sourceCommandType = typeof(RetrieveUserByCodeCommand);
        var sourceCommandTypeInController = controllerMethod.GetCustomAttribute<SourceCommandAttribute>()!.CommandType;

        // Assert
        Assert.Equal(sourceCommandType, sourceCommandTypeInController);
    }

    [Fact]
    public void SourceCommandAttribForQueryIsApplied_Success()
    {
        // Arrange
        var controllerType = typeof(UserResourceController);
        var controllerMethod = controllerType.GetMethod(nameof(UserResourceController.RetrieveNameByUserNameAsync))!;
        var sourceQueryType = typeof(RetrieveUserFullNameByNameQuery);
        var sourceQueryTypeInController = controllerMethod.GetCustomAttribute<SourceQueryAttribute>()!.QueryType;

        // Assert
        Assert.Equal(sourceQueryType, sourceQueryTypeInController);
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
        var content = await response.Content.ReadFromJsonAsync<RetrieveUserFullNameByNameQueryResult>();
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
        var response = await this.client.GetAsync("/users/id-5/active/is-active-true/related-users/key-3");

        // Assert
        var content = await response.Content.ReadFromJsonAsync<int[]>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new[] {5, 1, 3, 0}, content);
    }
    
    [Fact]
    public async Task ListUsers_WithQueryParametersForWrapperObject_Success()
    {
        // Act
        var response = await this.client.GetAsync("/users/id-5/active/is-active-true/related-users/key-3?filter.isSingle=true");

        // Assert
        var content = await response.Content.ReadFromJsonAsync<int[]>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new[] {5, 1, 3, 1}, content);
    }

    [Fact]
    public async Task CreateUser_Success()
    {
        // Act
        var response = await this.client.PostAsJsonAsync("/users", new
        {
            Name = "Angel",
            Addresses = new[] { new { City = "Madrid" } },
            Phones = new { },
        });

        // Assert
        var output = await response.Content.ReadFromJsonAsync<CreateUserOutput>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(output);
        Assert.Equal("Angel", output.Id);
    }

    [Fact]
    public async Task CreateUser_InvalidBody_Returns400WithAnErrorPerFieldByItsJsonPath()
    {
        // Act
        var response = await this.client.PostAsync("/users", new StringContent(
            """{"name": " ", "addresses": [{"city": "Madrid"}, {"city": ""}], "phones": {"Work line": {"number": ""}, "home": {"number": ""}}}""",
            Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.Equal(4, errors.Count);
        Assert.Equal(["A name is required."], errors["$.name"]);
        Assert.Equal(["A city is required."], errors["$.addresses[1].city"]);
        Assert.Equal(["A phone number is required."], errors["$.phones['Work line'].number"]);
        Assert.Equal(["A phone number is required."], errors["$.phones.home.number"]);
    }

    [Fact]
    public async Task CreateUser_UnreadableValue_Returns400WithThePathOfItsField()
    {
        // Act
        var response = await this.client.PostAsync("/users", new StringContent(
            """{"name": "Angel", "addresses": [{"city": 5}], "phones": {}}""", Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.Equal(["$.addresses[0].city"], errors.Keys);
    }

    [Fact]
    public async Task CreateUser_EmptyBody_Returns400()
    {
        // Act
        var response = await this.client.PostAsync("/users", new StringContent(string.Empty, Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ActionUser_ErrorAboutTheWholeCommand_Returns400UnderTheRoot()
    {
        // Act
        var response = await this.client.PatchAsync("/users/code-41/enable", JsonContent.Create(new
        {
            Enable = true,
            EnableName = "",
        }));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.Equal(["$"], errors.Keys);
        Assert.Equal(["A name is required to enable a user."], errors["$"]);
    }

    [Fact]
    public async Task RetrieveUser_InvalidKey_Returns400ByTheNameOfTheRouteParameter()
    {
        // Act
        var response = await this.client.GetAsync("/users/code-0");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.Equal(["code"], errors.Keys);
        Assert.Equal(["The code must be positive."], errors["code"]);
    }

    [Fact]
    public async Task ListSearchResults_AbsentRequiredQueryStringValues_Returns400ByTheirNames()
    {
        // Act
        var response = await this.client.GetAsync("/search-results");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.Equal(["page", "text"], errors.Keys.Order());
        Assert.Equal(["A value for 'page' was not provided."], errors["page"]);
        Assert.Equal(["A value for 'text' was not provided."], errors["text"]);
    }

    [Fact]
    public async Task ListSearchResults_UnreadableRequiredQueryStringValue_IsReportedOnlyAsUnreadable()
    {
        // Act
        var response = await this.client.GetAsync("/search-results?text=cats&page=abc");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.Equal(["page"], errors.Keys);
        Assert.Equal(["The value 'abc' is not valid for page."], errors["page"]);
    }

    [Fact]
    public async Task ListSearchResults_UnreadableValueAndAbsentRequiredValue_AreBothReported()
    {
        // Act
        var response = await this.client.GetAsync("/search-results?page=abc");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.Equal(["page", "text"], errors.Keys.Order());
        Assert.Equal(["The value 'abc' is not valid for page."], errors["page"]);
        Assert.Equal(["A value for 'text' was not provided."], errors["text"]);
    }

    [Fact]
    public async Task ListSearchResults_EmptyRequiredQueryStringValue_IsValidatedByTheCommand()
    {
        // Act
        var response = await this.client.GetAsync("/search-results?text=&page=1");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.Equal(["text"], errors.Keys);
        Assert.Equal(["A text to search is required."], errors["text"]);
    }

    [Fact]
    public async Task ListSearchResults_ComplexQueryStringValue_IsNotValidatedByMvc()
    {
        // Act
        var response = await this.client.GetAsync("/search-results?text=cats&page=1&options.page=2");

        // Assert
        var content = await response.Content.ReadFromJsonAsync<string>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("cats-1-null-2-[]-[]", content);
    }

    [Theory]
    [InlineData("tags=a&tags=b", "[a,b]-[]")]
    [InlineData("tags[0]=a&tags[1]=b", "[a,b]-[]")]
    [InlineData("tags[x]=a&tags[y]=b&tags.index=x&tags.index=y", "[a,b]-[]")]
    [InlineData("weights[x]=1&weights[y]=2", "[]-[x:1,y:2]")]
    [InlineData("weights[0].key=x&weights[0].value=1&weights[1].key=y&weights[1].value=2", "[]-[x:1,y:2]")]
    [InlineData("tags[1]=b&tags[0]=a", "[a,b]-[]")]
    [InlineData("weights[1]=2", "[]-[1:2]")]
    public async Task ListSearchResults_CollectionInEachFormatMvcAccepts_IsRead(string collectionQuery, string expectedCollections)
    {
        // Act
        var response = await this.client.GetAsync($"/search-results?text=cats&page=1&{collectionQuery}");

        // Assert
        var content = await response.Content.ReadFromJsonAsync<string>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal($"cats-1-null--{expectedCollections}", content);
    }

    [Fact]
    public async Task ListSearchResults_NestedCollectionWithConsecutiveIndexes_IsRead()
    {
        // Act
        var response = await this.client.GetAsync("/search-results?text=cats&page=1&options.labels[0]=a&options.labels[1]=b");

        // Assert
        var content = await response.Content.ReadFromJsonAsync<string>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("cats-1-null-0-[]-[]", content);
    }

    [Theory]
    [InlineData("tags[0]=a&tags[2]=c", "tags")]
    [InlineData("tags[0]=a&tags[1]=b&tags[3]=d", "tags")]
    [InlineData("tags[1]=b", "tags")]
    [InlineData("tags[x]=a", "tags")]
    [InlineData("weights[0].key=x&weights[0].value=1&weights[2].key=z&weights[2].value=3", "weights")]
    [InlineData("weights[1].key=y&weights[1].value=2", "weights")]
    [InlineData("options.labels[1]=b", "options.labels")]
    public async Task ListSearchResults_IndexesThatAreNotConsecutiveFromZero_Returns400(string collectionQuery, string expectedCollection)
    {
        // Act
        var response = await this.client.GetAsync($"/search-results?text=cats&page=1&{collectionQuery}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.Equal([expectedCollection], errors.Keys);
        Assert.Equal([$"The indexes of '{expectedCollection}' are not consecutive numbers from 0."], errors[expectedCollection]);
    }

    [Theory]
    [InlineData("text=cats&text=dogs&page=1", "text", "text")]
    [InlineData("text=cats&page=1&options.page=1&options.page=2", "options.Page", "options.page")]
    [InlineData("text=cats&page=1&tags[0]=a&tags[0]=b", "tags[0]", "tags[0]")]
    [InlineData("text=cats&page=1&weights[x]=1&weights[x]=2", "weights[x]", "weights[x]")]
    public async Task ListSearchResults_RepeatedNameOfAValueThatIsNotACollection_Returns400(string query, string expectedKey, string expectedName)
    {
        // Act
        var response = await this.client.GetAsync($"/search-results?{query}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.Equal([expectedKey], errors.Keys);
        Assert.Equal([$"Multiple values are not accepted for '{expectedName}'."], errors[expectedKey]);
    }

    [Fact]
    public async Task ListSearchResults_QueryStringValueNamedAsTheQueryStringParameter_IsReadByItsOwnName()
    {
        // Act
        var response = await this.client.GetAsync("/search-results?text=cats&page=1&queryString=x&options.page=abc");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.Equal(["options.Page"], errors.Keys);
    }

    [Fact]
    public async Task WithoutModelStateInvalidFilter_UnreadableValueAndAbsentRequiredValue_AreBothReported()
    {
        // Act
        var response = await this.clientWithoutModelStateInvalidFilter.GetAsync("/search-results?page=abc");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.Equal(["page", "text"], errors.Keys.Order());
        Assert.Equal(["The value 'abc' is not valid for page."], errors["page"]);
        Assert.Equal(["A value for 'text' was not provided."], errors["text"]);
    }

    [Fact]
    public async Task ListUsers_InvalidQueryParameter_Returns400ByTheNameOfTheQueryParameter()
    {
        // Act
        var response = await this.client.GetAsync("/users/by-key/key1-k1/key2-1?param1=101");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.Equal(["param1"], errors.Keys);
        Assert.Equal(["The first parameter cannot be greater than 100."], errors["param1"]);
    }

    [Fact]
    public async Task InvalidDataOfAnInnerCommand_IsNotAnErrorOfTheRequest()
    {
        // Act
        var response = await this.client.PatchAsync("/orders/invoke-invalid", null);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ByKey_Success()
    {
        // Arrange
        int id = 11;

        // Act
        var response = await this.client.PatchAsync($"/users/code-{id}", new StringContent(
            """{"updated": true, "nickname": null}""", Encoding.UTF8, "application/json"));

        // Assert
        var content = await response.Content.ReadFromJsonAsync<string>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("updated-11-True-null", content);
    }

    [Fact]
    public async Task UpdateUser_AbsentMembers_AreNotToBeChanged()
    {
        // Act
        var response = await this.client.PatchAsync("/users/code-12", new StringContent(
            """{"nickname": "Angie"}""", Encoding.UTF8, "application/json"));

        // Assert
        var content = await response.Content.ReadFromJsonAsync<string>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("updated-12-unchanged-Angie", content);
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
            Content = JsonContent.Create(new { Ids = new[] { 1, 2, 3 } })
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
        var response = await this.client.PatchAsync($"/users/name-{name}/rename", JsonContent.Create(new { NewName = "Miguel" }));

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
    public async Task OpenApi_CommandBodyHasTheDataOfTheCommandThatIsNotAKey()
    {
        // Act
        var response = await this.client.GetAsync("/openapi/v1.json");

        // Assert
        var stream = await response.Content.ReadAsStreamAsync();
        var document = (await OpenApiDocument.LoadAsync(stream)).Document!;
        var schema = document.Components!.Schemas!["UserActionEnableByCodeBody"];
        Assert.Equal(["enableName", "enable"], schema.Properties!.Keys);
        Assert.Equal(["enable", "enableName"], schema.Required!.Order());
        var parameters = document.Paths["/users/code-{code}/enable"].Operations![HttpMethod.Patch].Parameters!;
        Assert.Equal(["code"], parameters.Select(parameter => parameter.Name));
    }

    [Fact]
    public async Task OpenApi_UpdatableIsDescribedAsItsValue()
    {
        // Act
        var response = await this.client.GetAsync("/openapi/v1.json");

        // Assert
        var stream = await response.Content.ReadAsStreamAsync();
        var document = (await OpenApiDocument.LoadAsync(stream)).Document!;
        var schema = document.Components!.Schemas!["UserUpdateByCodeBody"];
        Assert.Equal(["updated", "nickname"], schema.Properties!.Keys);
        var updated = schema.Properties["updated"];
        var updatedSchema = updated is OpenApiSchemaReference reference ? reference.Target! : updated;
        Assert.Equal(JsonSchemaType.Boolean, Assert.Single(updatedSchema.AllOf!).Type);
    }

    [Fact]
    public void ResourceGroups_AreGeneratedInTheirOwnControllers()
    {
        // Act
        var defaultGroupMethods = typeof(OrderResourceController).GetMethods().Select(method => method.Name).ToArray();
        var publicGroupMethods = typeof(PublicOrderResourceController).GetMethods().Select(method => method.Name).ToArray();

        // Assert
        Assert.Contains(nameof(OrderResourceController.ListAsync), defaultGroupMethods);
        Assert.Contains(nameof(OrderResourceController.RetrieveByIdAsync), defaultGroupMethods);
        Assert.Contains(nameof(PublicOrderResourceController.ListAsync), publicGroupMethods);
        Assert.Contains(nameof(PublicOrderResourceController.RetrieveByIdAsync), publicGroupMethods);
        Assert.Equal("orders", typeof(PublicOrderResourceController).GetCustomAttribute<Microsoft.AspNetCore.Mvc.RouteAttribute>()!.Template);
    }

    [Theory]
    [InlineData("/orders", "order")]
    [InlineData("/public/orders", "public-order")]
    public async Task ListOrders_EachGroupIsServedWhereItsConventionSays_Success(string path, string expected)
    {
        // Act
        var response = await this.client.GetAsync(path);

        // Assert
        var content = await response.Content.ReadFromJsonAsync<string[]>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([expected], content!);
    }

    [Fact]
    public async Task RetrieveOrder_FromQueryInAGroup_Success()
    {
        // Act
        var defaultGroupResponse = await this.client.GetAsync("/orders/id-5");
        var publicGroupResponse = await this.client.GetAsync("/public/orders/id-5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, defaultGroupResponse.StatusCode);
        Assert.Equal("order-5", await defaultGroupResponse.Content.ReadFromJsonAsync<string>());
        Assert.Equal(HttpStatusCode.OK, publicGroupResponse.StatusCode);
        Assert.Equal("public-order-5", (await publicGroupResponse.Content.ReadFromJsonAsync<RetrievePublicOrderQueryResult>())!.Name);
    }

    [Fact]
    public void QueryParameterSetByMiddleware_IsNotPartOfTheAction()
    {
        // Act
        var parameters = typeof(TenantUserResourceController)
            .GetMethod(nameof(TenantUserResourceController.ListAsync))!
            .GetParameters()
            .Select(parameter => parameter.Name!)
            .ToArray();

        // Assert
        Assert.Equal(["prefix", "cancellationToken"], parameters);
    }

    [Fact]
    public async Task QueryParameterSetByMiddleware_Success()
    {
        // Act
        var response = await this.client.GetAsync("/tenant-users?prefix=user&tenant=other");

        // Assert
        var content = await response.Content.ReadFromJsonAsync<ListTenantUsersQueryResult[]>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([$"{TenantQueryMiddleware.Tenant}-user"], content!.Select(result => result.Name));
    }

    [Fact]
    public async Task OpenApi_CommandQueryStringHasTheDataOfTheCommandThatIsNotAKey()
    {
        // Act
        var response = await this.client.GetAsync("/openapi/v1.json");

        // Assert
        var stream = await response.Content.ReadAsStreamAsync();
        var document = (await OpenApiDocument.LoadAsync(stream)).Document!;
        var parameters = document.Paths["/users/by-key/key1-{key1}/key2-{key2}"].Operations![HttpMethod.Get].Parameters!;
        Assert.Equal(["key1", "key2", "param1", "param2"], parameters.Select(parameter => parameter.Name));
    }

    [Fact]
    public async Task QueryParameterSetByMiddleware_IsNotInTheApiDescription()
    {
        // Act
        var response = await this.client.GetAsync("/openapi/v1.json");

        // Assert
        var stream = await response.Content.ReadAsStreamAsync();
        var document = (await OpenApiDocument.LoadAsync(stream)).Document!;
        var parameters = document.Paths["/tenant-users"].Operations![HttpMethod.Get].Parameters!;
        Assert.Equal(["prefix"], parameters.Select(parameter => parameter.Name));
    }

    [Fact]
    public async Task ResourceNotFound_Command_Returns404()
    {
        // Arrange
        int id = 123;

        // Act
        var response = await this.client.GetAsync($"/not-found-commands/id-{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task ResourceNotFound_Query_Returns404()
    {
        // Arrange
        int id = 123;

        // Act
        var response = await this.client.GetAsync($"/not-found-queries/id-{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task WithoutModelStateInvalidFilter_UnreadableRouteValueOfACommand_Returns400ByTheNameOfTheRouteParameter()
    {
        // Act
        var response = await this.clientWithoutModelStateInvalidFilter.GetAsync("/users/code-abc");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.Equal(["code"], errors.Keys);
        Assert.Equal(["The value 'abc' is not valid."], errors["code"]);
    }

    [Fact]
    public async Task WithoutModelStateInvalidFilter_UnreadableQueryStringValueOfACommand_Returns400ByTheNameOfTheQueryParameter()
    {
        // Act
        var response = await this.clientWithoutModelStateInvalidFilter.GetAsync("/users/by-key/key1-k1/key2-1?param1=abc");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.Equal(["param1"], errors.Keys);
        Assert.Equal(["The value 'abc' is not valid for param1."], errors["param1"]);
    }

    [Fact]
    public async Task WithoutModelStateInvalidFilter_UnreadableRouteValueOfAQuery_Returns400ByTheNameOfTheRouteParameter()
    {
        // Act
        var response = await this.clientWithoutModelStateInvalidFilter.GetAsync("/not-found-queries/id-abc");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.Equal(["id"], errors.Keys);
        Assert.Equal(["The value 'abc' is not valid."], errors["id"]);
    }

    [Fact]
    public async Task WithoutModelStateInvalidFilter_EmptyBody_Returns400()
    {
        // Act
        var response = await this.clientWithoutModelStateInvalidFilter.PostAsync("/users", new StringContent(string.Empty, Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.Equal([""], errors.Keys);
        Assert.Equal(["A non-empty request body is required."], errors[""]);
    }

    [Fact]
    public async Task WithoutModelStateInvalidFilter_UnreadableBodyValue_Returns400WithThePathOfItsField()
    {
        // Act
        var response = await this.clientWithoutModelStateInvalidFilter.PostAsync("/users", new StringContent(
            """{"name": "Angel", "addresses": [{"city": 5}], "phones": {}}""", Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = await ReadErrorsAsync(response);
        Assert.Equal(["$.addresses[0].city"], errors.Keys);
    }

    [Fact]
    public async Task WithoutModelStateInvalidFilter_ValidRequest_Success()
    {
        // Act
        var response = await this.clientWithoutModelStateInvalidFilter.GetAsync("/users/code-5");

        // Assert
        var content = await response.Content.ReadFromJsonAsync<string>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("5", content);
    }

    private static async Task<Dictionary<string, string[]>> ReadErrorsAsync(HttpResponseMessage response)
    {
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>();
        return problem!.Errors;
    }

    private record ValidationProblem(Dictionary<string, string[]> Errors);
}
