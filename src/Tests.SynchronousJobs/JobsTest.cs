using System.Threading;
using System.Threading.Tasks;
using Cronos;
using IOKode.OpinionatedFramework.ConfigureApplication;
using IOKode.OpinionatedFramework.ContractImplementations.SynchronousJobs;
using IOKode.OpinionatedFramework.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.SynchronousJobs;

public class JobsTest
{
    private void InitLocator()
    {
        Container.Clear();
        Container.Services.AddTransient<IJobEnqueuer, SynchronousJobEnqueuer>();
        Container.Services.AddTransient<IJobScheduler, SynchronousJobScheduler>();
        Container.Services.AddSingleton<Counter>();
        Container.Initialize();
    }

    [Fact]
    public async Task EnqueueJob()
    {
        // Arrange
        InitLocator();
        var jobEnqueuer = Locator.Resolve<IJobEnqueuer>();
        var queue = new Queue();

        // Act
        await jobEnqueuer.EnqueueAsync(queue, new CountJob(), default);

        // Assert
        var counter = Locator.Resolve<Counter>();
        Assert.Equal(1, counter.Count);
    }

    [Fact]
    public async Task ScheduleJob()
    {
        // Arrange
        InitLocator();
        var jobScheduler = Locator.Resolve<IJobScheduler>();

        // Act
        await jobScheduler.ScheduleAsync(new CountJob(), CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(10000);

        // Assert
        var counter = Locator.Resolve<Counter>();
        Assert.Equal(5, counter.Count);
    }
    
    [Fact]
    public async Task RescheduleJob()
    {
        // Arrange
        InitLocator();
        var jobScheduler = Locator.Resolve<IJobScheduler>();

        // Act
        var scheduledJob = await jobScheduler.ScheduleAsync(new CountJob(), CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(6000);
        await jobScheduler.RescheduleAsync(scheduledJob, CronExpression.Parse("0/4 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(4000);

        // Assert
        var counter = Locator.Resolve<Counter>();
        Assert.Equal(4, counter.Count);
    }

    [Fact]
    public async Task UnscheduleJob()
    {
        // Arrange
        InitLocator();
        var jobScheduler = Locator.Resolve<IJobScheduler>();

        // Act
        var scheduledJob = await jobScheduler.ScheduleAsync(new CountJob(), CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(6000);
        await jobScheduler.UnscheduleAsync(scheduledJob, default);
        await Task.Delay(4000);

        // Assert
        var counter = Locator.Resolve<Counter>();
        Assert.Equal(3, counter.Count);
    }
}

public class Counter
{
    public int Count { get; set; }
}

public class CountJob : IJob // todo think another job for testing
{
    public Task InvokeAsync(CancellationToken cancellationToken)
    {
        var counter = Locator.Resolve<Counter>();
        counter.Count++;
        return Task.CompletedTask;
    }
}