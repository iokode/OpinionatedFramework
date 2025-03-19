using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Tests.Hangfire.Config;
using IOKode.OpinionatedFramework.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.Hangfire;

[Collection(nameof(JobsTestsFixtureCollection))]
public class JobsTest(JobsTestsFixture fixture, ITestOutputHelper output) : JobsTestsBase(fixture, output)
{
    [Fact]
    public async Task EnqueueJob_Success()
    {
        // Arrange
        var jobEnqueuer = new HangfireJobEnqueuer();

        // Act
        await jobEnqueuer.EnqueueAsync<EnqueuedJob>(Queue.Default, default);
        await PollingUtility.WaitUntilTrueAsync(() => EnqueuedJob.IsExecuted, 5000, 500);

        // Assert
        Assert.True(EnqueuedJob.IsExecuted);
    }

    [Fact]
    public async Task EnqueueJobWithDelay_Success()
    {
        // Arrange
        var jobEnqueuer = new HangfireJobEnqueuer();

        // Act
        await jobEnqueuer.EnqueueWithDelayAsync<EnqueuedWithDelayJob>(TimeSpan.FromMilliseconds(5000), Queue.Default, default);
        await PollingUtility.WaitUntilTrueAsync(() => EnqueuedWithDelayJob.IsExecuted, 20000, 1000);

        // Assert
        Assert.True(EnqueuedWithDelayJob.IsExecuted);
    }

    [Fact]
    public async Task ScheduleJob_Success()
    {
        // Arrange
        var jobScheduler = new HangfireJobScheduler();

        // Act
        var scheduledJob = await jobScheduler.ScheduleAsync<ScheduledJob>(CronExpression.Parse("0/5 * * * * *", CronFormat.IncludeSeconds), default);
        await PollingUtility.WaitUntilTrueAsync(() => ScheduledJob.Counter >= 2, 30000, 1000);
        await jobScheduler.UnscheduleAsync(scheduledJob, default);

        // Assert
        Assert.InRange(ScheduledJob.Counter, 2, 3);
    }

    [Fact]
    public async Task RescheduleJob_Success()
    {
        // Arrange
        var jobScheduler = new HangfireJobScheduler();

        // Act
        var scheduledJob = await jobScheduler.ScheduleAsync<RescheduledJob>(CronExpression.Parse("0/20 * * * *"), default);
        await Task.Delay(15000);
        await jobScheduler.RescheduleAsync(scheduledJob, CronExpression.Parse("0/15 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(16000);
        await jobScheduler.UnscheduleAsync(scheduledJob, default);

        // Assert
        Assert.True(RescheduledJob.IsExecuted);
    }

    [Fact]
    public async Task UnscheduleJob_Success()
    {
        // Arrange
        var jobScheduler = new HangfireJobScheduler();

        // Act
        var scheduledJob = await jobScheduler.ScheduleAsync<UnscheduledJob>(CronExpression.Parse("0 * * * *"), default);
        await jobScheduler.UnscheduleAsync(scheduledJob, default);
        await Task.Delay(5000);

        // Assert
        Assert.False(UnscheduledJob.IsExecuted);
    }
}

public class EnqueuedJob : Job
{
    public static bool IsExecuted;
    public override Task ExecuteAsync(IJobExecutionContext context)
    {
        IsExecuted = true;
        return Task.CompletedTask;
    }
}

public class EnqueuedWithDelayJob : Job
{
    public static bool IsExecuted;
    public override Task ExecuteAsync(IJobExecutionContext context)
    {
        IsExecuted = true;
        return Task.CompletedTask;
    }
}

public class ScheduledJob : Job
{
    public static int Counter;
    public override Task ExecuteAsync(IJobExecutionContext context)
    {
        Counter++;
        return Task.CompletedTask;
    }
}

public class RescheduledJob : Job
{
    public static bool IsExecuted;
    public override Task ExecuteAsync(IJobExecutionContext context)
    {
        IsExecuted = true;
        return Task.CompletedTask;
    }
}

public class UnscheduledJob : Job
{
    public static bool IsExecuted;
    public override Task ExecuteAsync(IJobExecutionContext context)
    {
        IsExecuted = true;
        return Task.CompletedTask;
    }
}