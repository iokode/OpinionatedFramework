using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Hangfire;
using IOKode.OpinionatedFramework.ContractImplementations.Hangfire;
using IOKode.OpinionatedFramework.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.Hangfire;

public class JobsTest
{
    [Fact]
    public async Task EnqueueJob()
    {
        // Arrange
        IConfiguration configuration = new ConfigurationBuilder()
            .AddUserSecrets<JobsTest>()
            .AddEnvironmentVariables()
            .Build();

        {
            var dbClient = new MongoClient(configuration["Hangfire:Mongo:ConnectionString"]);
            await dbClient.DropDatabaseAsync(configuration["Hangfire:Mongo:Database"]);
        }
        
        var services = new ServiceCollection();
        services.AddHangfire(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var jobEnqueuer = serviceProvider.GetService<IJobEnqueuer>();
        var countJob = new CountJob(3);
        using var server = new BackgroundJobServer();

        // Act
        await jobEnqueuer!.EnqueueAsync(Queue.Default, countJob, default);
        await Task.Delay(2000);

        // Assert
        Assert.Equal(1, Counter.Counts[3]);
    }
    
    [Fact]
    public async Task ScheduleJob_Success()
    {
        // Arrange
        IConfiguration configuration = new ConfigurationBuilder()
            .AddUserSecrets<JobsTest>()
            .AddEnvironmentVariables()
            .Build();
        var services = new ServiceCollection();
        services.AddHangfire(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var jobScheduler = serviceProvider.GetService<IJobScheduler>();
        var countJob = new CountJob(Counter.ScheduleSuccessCount);
        using var server = new BackgroundJobServer();

        // Act
        await jobScheduler!.ScheduleAsync(countJob, CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(10000);

        // Assert
        Assert.Equal(5, Counter.Counts[Counter.ScheduleSuccessCount]);
    }
}

public static class Counter
{
    public static readonly int[] Counts = new int[5];
    public static int EnqueueSuccessCount = 0;
    public static int ScheduleSuccessCount = 1;
}

public class CountJob : IJob
{
    public int Count { get; set; }
    
    public CountJob(int count)
    {
        this.Count = count;
    }

    public Task InvokeAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Running job");
        Counter.Counts[Count]++;
        return Task.CompletedTask;
    }
}