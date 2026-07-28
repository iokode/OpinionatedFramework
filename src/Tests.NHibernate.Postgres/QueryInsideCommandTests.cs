using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Persistence.Queries;
using IOKode.OpinionatedFramework.ServiceLocation;
using IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres;

/// <summary>
/// Regression tests for issue #118 (https://github.com/iokode/OpinionatedFramework/issues/118): the command executor
/// and the query executor each ask the container for a scope, so a query invoked from inside a command used to fail
/// because scopes could not be nested.
/// </summary>
[Collection(nameof(NHibernateTestsFixtureCollection))]
public class QueryInsideCommandTests(NHibernateTestsFixture fixture, ITestOutputHelper outputHelper)
    : NHibernateTestsBase(fixture, outputHelper)
{
    private static ICommandExecutor CreateCommandExecutor()
    {
        return new ContractImplementations.CommandExecutor.CommandExecutor(_ => { });
    }

    [Fact]
    public async Task InvokeQueryInsideCommand_Success()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Test User', 'test@example.com', true);");
        var commandExecutor = CreateCommandExecutor();

        // Act
        var users = await commandExecutor.InvokeAsync<QueryUsersCommand, IReadOnlyCollection<UserDto>>(
            new QueryUsersCommand(), CancellationToken.None);

        // Assert
        Assert.Single(users);
        Assert.Equal("Test User", users.First().Name);
    }

    [Fact]
    public async Task InvokeQueryInsideCommand_QueryParticipatesInTheCommandScope()
    {
        // Arrange
        await npgsqlClient.ExecuteAsync("INSERT INTO Users (id, name, email, is_active) VALUES ('1', 'Test User', 'test@example.com', true);");
        var commandExecutor = CreateCommandExecutor();

        // Act
        var (scopedServiceInCommand, scopedServiceInQueryMiddleware) = await commandExecutor
            .InvokeAsync<QueryUsersWithScopedServiceCommand, (ScopedService, ScopedService)>(
                new QueryUsersWithScopedServiceCommand(), CancellationToken.None);

        // Assert
        Assert.Same(scopedServiceInCommand, scopedServiceInQueryMiddleware);
    }

    private class QueryUsersCommand : Command<IReadOnlyCollection<UserDto>>
    {
        protected override async Task<IReadOnlyCollection<UserDto>> ExecuteAsync(ICommandExecutionContext executionContext)
        {
            var queryExecutor = Locator.Resolve<IQueryExecutor>();

            return await queryExecutor.QueryAsync<UserDto>(
                "SELECT id, name, email, is_active FROM Users", null,
                cancellationToken: executionContext.CancellationToken);
        }
    }

    private class QueryUsersWithScopedServiceCommand : Command<(ScopedService, ScopedService)>
    {
        protected override async Task<(ScopedService, ScopedService)> ExecuteAsync(ICommandExecutionContext executionContext)
        {
            var scopedService = Locator.Resolve<ScopedService>();

            var queryExecutorFactory = Locator.Resolve<IQueryExecutorFactory>();
            var queryExecutor = queryExecutorFactory.Create(typeof(ScopedServiceCapturingMiddleware));
            await queryExecutor.QueryAsync<UserDto>(
                "SELECT id, name, email, is_active FROM Users", null,
                cancellationToken: executionContext.CancellationToken);

            return (scopedService, ScopedServiceCapturingMiddleware.Captured!);
        }
    }

    public class UserDto
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Email { get; init; }
        public required bool IsActive { get; init; }
    }
}

public class ScopedServiceCapturingMiddleware : QueryMiddleware
{
    public static ScopedService? Captured { get; private set; }

    public override async Task ExecuteAsync(IQueryExecutionContext executionContext,
        Persistence.Queries.InvokeNextMiddlewareDelegate nextAsync)
    {
        Captured = Locator.Resolve<ScopedService>();

        await nextAsync();
    }
}
