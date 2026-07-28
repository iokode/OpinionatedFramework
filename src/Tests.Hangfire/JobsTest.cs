using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Jobs.Extensions;
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
        // Act
        await new EnqueueJobCreator().EnqueueAsync(CancellationToken.None);
        await PollingUtility.WaitUntilTrueAsync(() => EnqueuedJob.IsExecuted, 5000, 500);

        // Assert
        Assert.True(EnqueuedJob.IsExecuted);
    }

    /// <summary>
    /// The worker serves the queues named in configuration and no others.
    /// </summary>
    /// <remarks>
    /// Both jobs are enqueued at the same time and differ only in their queue, so the unserved one has had at
    /// least as long to run as the served one by the time the assertions execute. Asserting that the served job
    /// ran is not evidence on its own, because a job that ignored its queue would run just the same.
    /// </remarks>
    [Fact]
    public async Task WorkerServesTheConfiguredQueuesOnly()
    {
        // Act
        // Hangfire:Queues is ["default", "events"], so "reports" has no worker fetching from it.
        await new EventsQueueJobCreator().EnqueueAsync(Queue.Create("events"), CancellationToken.None);
        await new ReportsQueueJobCreator().EnqueueAsync(Queue.Create("reports"), CancellationToken.None);
        await PollingUtility.WaitUntilTrueAsync(() => EventsQueueJob.IsExecuted, 5000, 500);

        // Assert
        Assert.True(EventsQueueJob.IsExecuted);
        Assert.False(ReportsQueueJob.IsExecuted);
    }

    [Fact]
    public async Task EnqueueJobWithDelay_Success()
    {
        // Act
        await new EnqueuedWithDelayJobCreator().EnqueueWithDelayAsync(TimeSpan.FromMilliseconds(5000), CancellationToken.None);
        await PollingUtility.WaitUntilTrueAsync(() => EnqueuedWithDelayJob.IsExecuted, 20000, 1000);

        // Assert
        Assert.True(EnqueuedWithDelayJob.IsExecuted);
    }

    [Fact]
    public async Task ScheduleJob_Success()
    {
        // Act
        var scheduledJobId = await new ScheduledJobCreator().ScheduleAsync(CronExpression.Parse("0/5 * * * * *", CronFormat.IncludeSeconds));
        await PollingUtility.WaitUntilTrueAsync(() => ScheduledJob.Counter >= 2, 30000, 1000);
        await Facades.Job.UnscheduleAsync(scheduledJobId, CancellationToken.None);

        // Assert
        Assert.InRange(ScheduledJob.Counter, 2, 3);
    }

    [Fact]
    public async Task RescheduleJob_Success()
    {
        // Act
        var scheduledJobId = await new RescheduledJobCreator().ScheduleAsync(CronExpression.Parse("0/20 * * * *"));
        await Task.Delay(15000);
        await Facades.Job.RescheduleAsync(scheduledJobId, CronExpression.Parse("0/15 * * * * *", CronFormat.IncludeSeconds), CancellationToken.None);
        await Task.Delay(16000);
        await Facades.Job.UnscheduleAsync(scheduledJobId, CancellationToken.None);

        // Assert
        Assert.True(RescheduledJob.IsExecuted);
    }

    [Fact]
    public async Task UnscheduleJob_Success()
    {
        // Act
        var scheduledJobId = await new UnscheduledJobCreator().ScheduleAsync(CronExpression.Parse("0 * * * *"));
        await Facades.Job.UnscheduleAsync(scheduledJobId, CancellationToken.None);
        await Task.Delay(5000);

        // Assert
        Assert.False(UnscheduledJob.IsExecuted);
    }
}

public record EnqueueJobCreator : JobCreator<EnqueuedJob>
{
    public override EnqueuedJob CreateJob()
    {
        return new EnqueuedJob();
    }

    public override string GetJobName()
    {
        return "EnqueueJob";
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

public record EventsQueueJobCreator : JobCreator<EventsQueueJob>
{
    public override EventsQueueJob CreateJob()
    {
        return new EventsQueueJob();
    }

    public override string GetJobName()
    {
        return "EventsQueueJob";
    }
}

/// <summary>
/// Runs on the <c>events</c> queue, which the worker serves only because it is in the configured queues.
/// </summary>
public class EventsQueueJob : Job
{
    public static bool IsExecuted;
    public override Task ExecuteAsync(IJobExecutionContext context)
    {
        IsExecuted = true;
        return Task.CompletedTask;
    }
}

public record ReportsQueueJobCreator : JobCreator<ReportsQueueJob>
{
    public override ReportsQueueJob CreateJob()
    {
        return new ReportsQueueJob();
    }

    public override string GetJobName()
    {
        return "ReportsQueueJob";
    }
}

/// <summary>
/// Enqueued to the <c>reports</c> queue, which is absent from the configured queues, so it must never run.
/// </summary>
public class ReportsQueueJob : Job
{
    public static bool IsExecuted;
    public override Task ExecuteAsync(IJobExecutionContext context)
    {
        IsExecuted = true;
        return Task.CompletedTask;
    }
}

public record EnqueuedWithDelayJobCreator : JobCreator<EnqueuedWithDelayJob>
{
    public override EnqueuedWithDelayJob CreateJob()
    {
        return new EnqueuedWithDelayJob();
    }

    public override string GetJobName()
    {
        return "EnqueuedWithDelayJob";
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

public record ScheduledJobCreator : JobCreator<ScheduledJob>
{
    public override ScheduledJob CreateJob()
    {
        return new ScheduledJob();
    }

    public override string GetJobName()
    {
        return "ScheduledJob";
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

public record RescheduledJobCreator : JobCreator<RescheduledJob>
{
    public override RescheduledJob CreateJob()
    {
        return new RescheduledJob();
    }

    public override string GetJobName()
    {
        return "RescheduledJob";
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

public record UnscheduledJobCreator : JobCreator<UnscheduledJob>
{
    public override UnscheduledJob CreateJob()
    {
        return new UnscheduledJob();
    }

    public override string GetJobName()
    {
        return "UnscheduledJob";
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