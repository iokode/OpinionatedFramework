using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.TaskRunJobs;

public class JobsTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task EnqueueJob_Success()
    {
        // Arrange
        var configurationValues = new Dictionary<string, object>
        {
            ["TaskRun:JobEnqueuer:MaxAttempts"] = 10
        };
        var configuration = new ConfigurationProvider(configurationValues);
        var jobEnqueuer = new TaskRunJobEnqueuer(configuration);
        var countJob = new CountJob();

        // Act
        await jobEnqueuer.EnqueueAsync(Queue.Default, countJob, default);

        // Assert
        Assert.Equal(1, countJob.Counter.Count);
    }

    [Fact]
    public async Task ScheduleJob_Success()
    {
        // Arrange
        var configurationValues = new Dictionary<string, object>
        {
            ["TaskRun:JobScheduler:MaxAttempts"] = 10
        };
        var configuration = new ConfigurationProvider(configurationValues);
        var logging = new XUnitLogging(testOutputHelper);
        var jobScheduler = new TaskRunJobScheduler(configuration, logging);
        var countJob = new CountJob();

        // Act
        await jobScheduler.ScheduleAsync(countJob, CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(10000);

        // Assert
        Assert.Equal(5, countJob.Counter.Count);
    }

    [Fact]
    public async Task RescheduleJob_Success()
    {
        // Arrange
        var configurationValues = new Dictionary<string, object>
        {
            ["TaskRun:JobScheduler:MaxAttempts"] = 10
        };
        var configuration = new ConfigurationProvider(configurationValues);
        var logging = new XUnitLogging(testOutputHelper);
        var jobScheduler = new TaskRunJobScheduler(configuration, logging);
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
    public async Task UnscheduleJob_Success()
    {
        // Arrange
        var configurationValues = new Dictionary<string, object>
        {
            ["TaskRun:JobScheduler:MaxAttempts"] = 10
        };
        var configuration = new ConfigurationProvider(configurationValues);
        var logging = new XUnitLogging(testOutputHelper);
        var jobScheduler = new TaskRunJobScheduler(configuration, logging);
        var countJob = new CountJob();

        // Act
        var scheduledJob = await jobScheduler.ScheduleAsync(countJob, CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(6000);
        await jobScheduler.UnscheduleAsync(scheduledJob, default);
        await Task.Delay(4000);

        // Assert
        Assert.Equal(3, countJob.Counter.Count);
    }

    [Fact]
    public async Task EnqueueJob_FailAndRetry()
    {
        // Arrange
        var configurationValues = new Dictionary<string, object>
        {
            ["TaskRun:JobEnqueuer:MaxAttempts"] = 10
        };
        var configuration = new ConfigurationProvider(configurationValues);
        var jobEnqueuer = new TaskRunJobEnqueuer(configuration);
        var countJob = new CountFailJob();

        // Act
        await jobEnqueuer.EnqueueAsync(Queue.Default, countJob, default);
        await Task.Delay(2000);

        // Assert
        Assert.Equal(10, countJob.Counter.Count);
    }

    [Fact]
    public async Task ScheduleJob_FailAndRetry()
    {
        // Arrange
        var configurationValues = new Dictionary<string, object>
        {
            ["TaskRun:JobScheduler:MaxAttempts"] = 10
        };
        var configuration = new ConfigurationProvider(configurationValues);
        var logging = new XUnitLogging(testOutputHelper);
        var jobScheduler = new TaskRunJobScheduler(configuration, logging);
        var countJob = new CountFailJob();

        // Act
        await jobScheduler.ScheduleAsync(countJob, CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(10_000);

        // Assert
        // The job was executed 5 times (every 2 seconds in 10 seconds) and retried 10 times each execution, so 5*10 = 50.
        Assert.Equal(50, countJob.Counter.Count);
    }

    [Fact]
    public async Task EnqueueJob_FailAndRetryUntilSixthAttempt()
    {
        // Arrange
        var configurationValues = new Dictionary<string, object>
        {
            ["TaskRun:JobEnqueuer:MaxAttempts"] = 10
        };
        var configuration = new ConfigurationProvider(configurationValues);
        var jobEnqueuer = new TaskRunJobEnqueuer(configuration);
        var countJob = new CountFailUntilSixthAttemptJob();

        // Act
        await jobEnqueuer.EnqueueAsync(Queue.Default, countJob, default);
        await Task.Delay(2000);

        // Assert
        Assert.Equal(6, countJob.Counter.Count);
    }

    [Fact]
    public async Task ScheduleJob_FailAndRetryUntilSixthAttempt()
    {
        // Arrange
        var configurationValues = new Dictionary<string, object>
        {
            ["TaskRun:JobScheduler:MaxAttempts"] = 10
        };
        var configuration = new ConfigurationProvider(configurationValues);
        var logging = new XUnitLogging(testOutputHelper);
        var jobScheduler = new TaskRunJobScheduler(configuration, logging);
        var countJob = new CountFailUntilSixthAttemptJob();

        // Act
        await jobScheduler.ScheduleAsync(countJob, CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(10000);

        // Assert
        // The job first was retried 5 times and after that executed with success 5 times (every 2 seconds in 10 seconds), so 5+5 = 10.
        Assert.Equal(10, countJob.Counter.Count);
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

public class CountFailJob : IJob
{
    public Counter Counter { get; }

    public CountFailJob()
    {
        this.Counter = new Counter();
    }

    public Task InvokeAsync(CancellationToken cancellationToken)
    {
        this.Counter.Count++;
        throw new Exception($"Exception after increment counter. Count: {this.Counter.Count}");
    }
}

public class CountFailUntilSixthAttemptJob : IJob
{
    public Counter Counter { get; }

    public CountFailUntilSixthAttemptJob()
    {
        this.Counter = new Counter();
    }

    public Task InvokeAsync(CancellationToken cancellationToken)
    {
        this.Counter.Count++;
        if (this.Counter.Count < 6)
        {
            throw new Exception($"Exception after increment counter. Count: {this.Counter.Count}");
        }

        return Task.CompletedTask;
    }
}