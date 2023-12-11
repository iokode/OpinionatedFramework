using System.Threading;
using System.Threading.Tasks;
using Cronos;
using IOKode.OpinionatedFramework.ContractImplementations.SynchronousJobs;
using IOKode.OpinionatedFramework.Jobs;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.SynchronousJobs;

public class JobsTest
{
    [Fact]
    public async Task EnqueueJob()
    {
        // Arrange
        var jobEnqueuer = new SynchronousJobEnqueuer();
        var countJob = new CountJob();

        // Act
        await jobEnqueuer.EnqueueAsync(Queue.Default, countJob, default);

        // Assert
        Assert.Equal(1, countJob.Counter.Count);
    }

    [Fact]
    public async Task ScheduleJob()
    {
        // Arrange
        var jobScheduler = new SynchronousJobScheduler();
        var countJob = new CountJob();

        // Act
        await jobScheduler.ScheduleAsync(countJob, CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(10000);

        // Assert
        Assert.Equal(5, countJob.Counter.Count);
    }
    
    [Fact]
    public async Task RescheduleJob()
    {
        // Arrange
        var jobScheduler = new SynchronousJobScheduler();
        var countJob = new CountJob();

        // Act
        var scheduledJob = await jobScheduler.ScheduleAsync(countJob, CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(6000);
        await jobScheduler.RescheduleAsync(scheduledJob, CronExpression.Parse("0/4 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(4000);

        // Assert
        Assert.Equal(4, countJob.Counter.Count);
    }

    [Fact]
    public async Task UnscheduleJob()
    {
        // Arrange
        var jobScheduler = new SynchronousJobScheduler();
        var countJob = new CountJob();

        // Act
        var scheduledJob = await jobScheduler.ScheduleAsync(countJob, CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(6000);
        await jobScheduler.UnscheduleAsync(scheduledJob, default);
        await Task.Delay(4000);

        // Assert
        Assert.Equal(3, countJob.Counter.Count);
    }
}

public class Counter
{
    public int Count { get; set; }
}

public class CountJob : IJob
{
    public Counter Counter { get; }

    public CountJob()
    {
        this.Counter = new Counter();
    }

    public Task InvokeAsync(CancellationToken cancellationToken)
    {
        this.Counter.Count++;
        return Task.CompletedTask;
    }
}