using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;
using IOKode.OpinionatedFramework.Jobs;
using IOKode.OpinionatedFramework.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace IOKode.OpinionatedFramework.Tests.TaskRunJobs;

public class JobsTest : IDisposable
{
    private readonly ITestOutputHelper testOutputHelper;

    public JobsTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
        Container.Initialize();
    }
    
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
        await jobEnqueuer.EnqueueAsync(Queue.Default, new EnqueueJobCreator());

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
        await jobScheduler.ScheduleAsync(CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), new ScheduleJobCreator());
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
        var scheduledJob = await jobScheduler.ScheduleAsync(CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), new RescheduleJobCreator());
        await Task.Delay(4000);
        await jobScheduler.RescheduleAsync(scheduledJob, CronExpression.Parse("0/3 * * * * *", CronFormat.IncludeSeconds), CancellationToken.None);
        await Task.Delay(6000);

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
        var scheduledJob = await jobScheduler.ScheduleAsync(CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), new UnscheduleJobCreator());
        await Task.Delay(6000);
        await jobScheduler.UnscheduleAsync(scheduledJob, CancellationToken.None);
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
        await jobEnqueuer.EnqueueAsync(Queue.Default, new EnqueueFailAndRetryJobCreator());
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
        await jobScheduler.ScheduleAsync(CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), new ScheduleFailAndRetryJobCreator());
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
        await jobEnqueuer.EnqueueAsync(Queue.Default, new EnqueueFailAndRetryUntilSixthAttemptJobCreator());
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
        await jobScheduler.ScheduleAsync(CronExpression.Parse("0/2 * * * * *", CronFormat.IncludeSeconds), new ScheduleFailAndRetryUntilSixthAttemptJobCreator());
        await Task.Delay(10000);

        // Assert
        // The job first was retried 5 times and after that executed with success 5 times (every 2 seconds in 10 seconds), so 5+5 = 10.
        Assert.Equal(10, ScheduleFailAndRetryUntilSixthAttemptJob.Counter);
    }

    public void Dispose()
    {
        Container.Advanced.Clear();
    }
}

public record EnqueueJobCreator : JobCreator<EnqueueJob>
{
    public override EnqueueJob CreateJob()
    {
        return new EnqueueJob();
    }

    public override string GetJobName()
    {
        return "EnqueueJob";
    }
}

public class EnqueueJob : Job
{
    public static int Counter;

    public override Task ExecuteAsync(IJobExecutionContext context)
    {
        Counter++;
        return Task.CompletedTask;
    }
}

public record ScheduleJobCreator : JobCreator<ScheduleJob>
{
    public override ScheduleJob CreateJob()
    {
        return new ScheduleJob();
    }

    public override string GetJobName()
    {
        return "ScheduleJob";
    }
}

public class ScheduleJob : Job
{
    public static int Counter;

    public override Task ExecuteAsync(IJobExecutionContext context)
    {
        Counter++;
        return Task.CompletedTask;
    }
}

public record RescheduleJobCreator : JobCreator<RescheduleJob>
{
    public override RescheduleJob CreateJob()
    {
        return new RescheduleJob();
    }

    public override string GetJobName()
    {
        return "RescheduleJob";
    }
}

public class RescheduleJob : Job
{
    public static int Counter;

    public override Task ExecuteAsync(IJobExecutionContext context)
    {
        Counter++;
        return Task.CompletedTask;
    }
}

public record UnscheduleJobCreator : JobCreator<UnscheduleJob>
{
    public override UnscheduleJob CreateJob()
    {
        return new UnscheduleJob();
    }

    public override string GetJobName()
    {
        return "UnscheduleJob";
    }
}

public class UnscheduleJob : Job
{
    public static int Counter;

    public override Task ExecuteAsync(IJobExecutionContext context)
    {
        Counter++;
        return Task.CompletedTask;
    }
}

public record EnqueueFailAndRetryJobCreator : JobCreator<EnqueueFailAndRetryJob>
{
    public override EnqueueFailAndRetryJob CreateJob()
    {
        return new EnqueueFailAndRetryJob();
    }

    public override string GetJobName()
    {
        return "EnqueueFailAndRetryJob";
    }
}

public class EnqueueFailAndRetryJob : Job
{
    public static int Counter;

    public override Task ExecuteAsync(IJobExecutionContext context)
    {
        Counter++;
        throw new Exception($"Exception after increment counter. Count: {Counter}");
    }
}

public record ScheduleFailAndRetryJobCreator : JobCreator<ScheduleFailAndRetryJob>
{
    public override ScheduleFailAndRetryJob CreateJob()
    {
        return new ScheduleFailAndRetryJob();
    }

    public override string GetJobName()
    {
        return "ScheduleFailAndRetryJob";
    }
}

public class ScheduleFailAndRetryJob : Job
{
    public static int Counter;

    public override Task ExecuteAsync(IJobExecutionContext context)
    {
        Counter++;
        throw new Exception($"Exception after increment counter. Count: {Counter}");
    }
}

public record EnqueueFailAndRetryUntilSixthAttemptJobCreator : JobCreator<EnqueueFailAndRetryUntilSixthAttemptJob>
{
    public override EnqueueFailAndRetryUntilSixthAttemptJob CreateJob()
    {
        return new EnqueueFailAndRetryUntilSixthAttemptJob();
    }

    public override string GetJobName()
    {
        return "EnqueueFailAndRetryUntilSixthAttemptJob";
    }
}

public class EnqueueFailAndRetryUntilSixthAttemptJob : Job
{
    public static int Counter;

    public override Task ExecuteAsync(IJobExecutionContext context)
    {
        Counter++;
        if (Counter < 6)
        {
            throw new Exception($"Exception after increment counter. Count: {Counter}");
        }

        return Task.CompletedTask;
    }
}

public record ScheduleFailAndRetryUntilSixthAttemptJobCreator : JobCreator<ScheduleFailAndRetryUntilSixthAttemptJob>
{
    public override ScheduleFailAndRetryUntilSixthAttemptJob CreateJob()
    {
        return new ScheduleFailAndRetryUntilSixthAttemptJob();
    }

    public override string GetJobName()
    {
        return "ScheduleFailAndRetryUntilSixthAttemptJob";
    }
}

public class ScheduleFailAndRetryUntilSixthAttemptJob : Job
{
    public static int Counter;

    public override Task ExecuteAsync(IJobExecutionContext context)
    {
        Counter++;
        if (Counter < 6)
        {
            throw new Exception($"Exception after increment counter. Count: {Counter}");
        }

        return Task.CompletedTask;
    }
}