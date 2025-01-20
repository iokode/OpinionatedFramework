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

        // Act
        await jobEnqueuer.EnqueueAsync<EnqueueJob>(Queue.Default, default);

        // Assert
        Assert.Equal(1, EnqueueJob.Counter);
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

        // Act
        await jobScheduler.ScheduleAsync<ScheduleJob>(CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(10000);

        // Assert
        Assert.Equal(5, ScheduleJob.Counter);
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

        // Act
        var scheduledJob = await jobScheduler.ScheduleAsync<RescheduleJob>(CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(6000);
        await jobScheduler.RescheduleAsync(scheduledJob, CronExpression.Parse("0/4 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(4000);

        // Assert
        Assert.Equal(4, RescheduleJob.Counter);
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

        // Act
        var scheduledJob = await jobScheduler.ScheduleAsync<UnscheduleJob>(CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(6000);
        await jobScheduler.UnscheduleAsync(scheduledJob, default);
        await Task.Delay(4000);

        // Assert
        Assert.Equal(3, UnscheduleJob.Counter);
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

        // Act
        await jobEnqueuer.EnqueueAsync<EnqueueFailAndRetryJob>(Queue.Default, default);
        await Task.Delay(2000);

        // Assert
        Assert.Equal(10, EnqueueFailAndRetryJob.Counter);
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

        // Act
        await jobScheduler.ScheduleAsync<ScheduleFailAndRetryJob>(CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(10_000);

        // Assert
        // The job was executed 5 times (every 2 seconds in 10 seconds) and retried 10 times each execution, so 5*10 = 50.
        Assert.Equal(50, ScheduleFailAndRetryJob.Counter);
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

        // Act
        await jobEnqueuer.EnqueueAsync<EnqueueFailAndRetryUntilSixthAttemptJob>(Queue.Default, default);
        await Task.Delay(2000);

        // Assert
        Assert.Equal(6, EnqueueFailAndRetryUntilSixthAttemptJob.Counter);
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

        // Act
        await jobScheduler.ScheduleAsync<ScheduleFailAndRetryUntilSixthAttemptJob>(CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), default);
        await Task.Delay(10000);

        // Assert
        // The job first was retried 5 times and after that executed with success 5 times (every 2 seconds in 10 seconds), so 5+5 = 10.
        Assert.Equal(10, ScheduleFailAndRetryUntilSixthAttemptJob.Counter);
    }
}

public class EnqueueJob : IJob
{
    public static int Counter;

    public Task InvokeAsync(CancellationToken cancellationToken)
    {
        Counter++;
        return Task.CompletedTask;
    }
}

public class ScheduleJob : IJob
{
    public static int Counter;

    public Task InvokeAsync(CancellationToken cancellationToken)
    {
        Counter++;
        return Task.CompletedTask;
    }
}

public class RescheduleJob : IJob
{
    public static int Counter;

    public Task InvokeAsync(CancellationToken cancellationToken)
    {
        Counter++;
        return Task.CompletedTask;
    }
}

public class UnscheduleJob : IJob
{
    public static int Counter;

    public Task InvokeAsync(CancellationToken cancellationToken)
    {
        Counter++;
        return Task.CompletedTask;
    }
}

public class EnqueueFailAndRetryJob : IJob
{
    public static int Counter;

    public Task InvokeAsync(CancellationToken cancellationToken)
    {
        Counter++;
        throw new Exception($"Exception after increment counter. Count: {Counter}");
    }
}

public class ScheduleFailAndRetryJob : IJob
{
    public static int Counter;

    public Task InvokeAsync(CancellationToken cancellationToken)
    {
        Counter++;
        throw new Exception($"Exception after increment counter. Count: {Counter}");
    }
}

public class EnqueueFailAndRetryUntilSixthAttemptJob : IJob
{
    public static int Counter;

    public Task InvokeAsync(CancellationToken cancellationToken)
    {
        Counter++;
        if (Counter < 6)
        {
            throw new Exception($"Exception after increment counter. Count: {Counter}");
        }

        return Task.CompletedTask;
    }
}

public class ScheduleFailAndRetryUntilSixthAttemptJob : IJob
{
    public static int Counter;

    public Task InvokeAsync(CancellationToken cancellationToken)
    {
        Counter++;
        if (Counter < 6)
        {
            throw new Exception($"Exception after increment counter. Count: {Counter}");
        }

        return Task.CompletedTask;
    }
}