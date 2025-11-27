using System;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.Events.Config;

public class EventsTestsBase(EventsTestsFixture fixture) : IClassFixture<EventsTestsFixture>, IAsyncLifetime
{
    protected NpgsqlConnection npgsqlClient => fixture.NpgsqlClient ?? throw new NullReferenceException("NpgsqlClient is null. Did you forget to initialize the fixture?");

    protected async Task TruncateEventsTableAsync()
    {
        npgsqlClient.Open();
        await npgsqlClient.ExecuteAsync("TRUNCATE TABLE opinionated_framework.events;");
        await npgsqlClient.CloseAsync();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await TruncateEventsTableAsync();
    }
}