using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.Events.Config;

public class EventsTestsBase(EventsTestsFixture fixture) : IClassFixture<EventsTestsFixture>, IAsyncLifetime
{
    protected NpgsqlConnection npgsqlClient => fixture.NpgsqlClient;

    protected async Task TruncateEventsTableAsync()
    {
        npgsqlClient.Open();
        await npgsqlClient.ExecuteAsync("TRUNCATE TABLE opinionated_framework.events;");
        npgsqlClient.Close();
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