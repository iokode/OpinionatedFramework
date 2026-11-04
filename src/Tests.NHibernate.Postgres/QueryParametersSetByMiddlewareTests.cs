using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dapper;
using IOKode.OpinionatedFramework.Persistence.Queries;
using IOKode.OpinionatedFramework.ServiceLocation;
using IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config;
using IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config.Queries;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres;

[Collection(nameof(NHibernateTestsFixtureCollection))]
public class QueryParametersSetByMiddlewareTests(NHibernateTestsFixture fixture, ITestOutputHelper outputHelper)
    : NHibernateTestsBase(fixture, outputHelper)
{
    private static IQueryExecutor CreateExecutor(params Type[] middlewareTypes)
    {
        return Locator.Resolve<IQueryExecutorFactory>().Create(middlewareTypes);
    }

    [Fact]
    public void GeneratedProperties_AreMarkedAndNotRequired()
    {
        foreach (var type in new[] { typeof(GetCurrentUserNameQuery), typeof(GetCurrentUserNameQueryParameters) })
        {
            var id = type.GetProperty("Id")!;
            var name = type.GetProperty("Name")!;

            Assert.NotNull(id.GetCustomAttribute<SetByMiddlewareAttribute>());
            Assert.Null(id.GetCustomAttribute<RequiredMemberAttribute>());
            Assert.Null(name.GetCustomAttribute<SetByMiddlewareAttribute>());
            Assert.NotNull(name.GetCustomAttribute<RequiredMemberAttribute>());
        }
    }

    [Fact]
    public async Task ParameterSetByMiddleware_Success()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES ('1', 'Ivan'), ('2', 'Ivan');");
        CurrentUserMiddleware.CurrentUserId = "1";
        var queryExecutor = CreateExecutor(typeof(CurrentUserMiddleware));

        // Act
        var result = await queryExecutor.InvokeAsync(new GetCurrentUserNameQuery { Name = "Ivan" }, default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Ivan", result.Name);
    }

    [Fact]
    public async Task ParameterSetByMiddleware_ReplacesTheValueGivenToTheQuery()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES ('1', 'Ivan');");
        CurrentUserMiddleware.CurrentUserId = "2";
        var queryExecutor = CreateExecutor(typeof(CurrentUserMiddleware));

        // Act
        var result = await queryExecutor.InvokeAsync(new GetCurrentUserNameQuery { Id = "1", Name = "Ivan" }, default);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ParameterGivenToTheQuery_WithoutMiddleware_Success()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("INSERT INTO users (id, name) VALUES ('1', 'Ivan');");
        var queryExecutor = CreateExecutor();

        // Act
        var result = await queryExecutor.InvokeAsync(new GetCurrentUserNameQuery { Id = "1", Name = "Ivan" }, default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Ivan", result.Name);
    }

    [Fact]
    public async Task ParameterNotSet_Throws()
    {
        // Arrange
        var queryExecutor = CreateExecutor();

        // Act and Assert
        var exception = await Assert.ThrowsAsync<QueryParameterNotSetException>(() =>
            queryExecutor.InvokeAsync(new GetCurrentUserNameQuery { Name = "Ivan" }, default));
        Assert.Equal("Id", exception.ParameterName);
    }

    private class CurrentUserMiddleware : QueryMiddleware
    {
        public static string CurrentUserId { get; set; } = string.Empty;

        public override async Task ExecuteAsync(IQueryExecutionContext executionContext, InvokeNextMiddlewareDelegate nextAsync)
        {
            if (executionContext.HasParameter("id"))
            {
                executionContext.SetParameter("id", CurrentUserId);
            }

            await nextAsync();
        }
    }
}
