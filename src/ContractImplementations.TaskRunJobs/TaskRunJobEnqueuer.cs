using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Configuration;
using IOKode.OpinionatedFramework.Jobs;
using Microsoft.Extensions.Hosting;
using Job = IOKode.OpinionatedFramework.Jobs.Job;

namespace IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;

public class TaskRunJobEnqueuer(IConfigurationProvider configuration) : IJobEnqueuer, IHostedService, IAsyncDisposable
{
    private readonly Lock sync = new();
    private readonly HashSet<Task> runningTasks = [];
    private readonly CancellationTokenSource lifetimeCancellationTokenSource = new();
    private bool acceptsJobs = true;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (this.sync)
        {
            if (this.lifetimeCancellationTokenSource.IsCancellationRequested)
            {
                throw new InvalidOperationException("The job enqueuer cannot be restarted after it has been stopped.");
            }

            this.acceptsJobs = true;
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return this.DisposeAsync().AsTask();
    }

    public async ValueTask DisposeAsync()
    {
        Task[] tasks;
        lock (this.sync)
        {
            this.acceptsJobs = false;
            tasks = this.runningTasks.ToArray();
        }

        await this.lifetimeCancellationTokenSource.CancelAsync();
        if (tasks.Length == 0)
        {
            return;
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException) when (this.lifetimeCancellationTokenSource.IsCancellationRequested)
        {
        }
    }

    public Task EnqueueAsync<TJob>(Queue queue, JobCreator<TJob> creator,
        CancellationToken cancellationToken = default) where TJob : Job
    {
        cancellationToken.ThrowIfCancellationRequested();
        this.EnsureAcceptingJobs();
        var task = RetryHelper.RetryOnExceptionAsync(
            creator,
            configuration.GetValue<int>("OpinionatedFramework:JobEnqueuer:MaxAttempts"),
            this.lifetimeCancellationTokenSource.Token);
        this.Track(task);
        return Task.CompletedTask;
    }

    public Task EnqueueWithDelayAsync<TJob>(TimeSpan delay, Queue queue, JobCreator<TJob> creator,
        CancellationToken cancellationToken = default) where TJob : Job
    {
        cancellationToken.ThrowIfCancellationRequested();
        this.EnsureAcceptingJobs();
        var task = RunDelayedJobAsync(delay, creator, cancellationToken);
        this.Track(task);
        return Task.CompletedTask;
    }

    private async Task RunDelayedJobAsync<TJob>(TimeSpan delay, JobCreator<TJob> creator,
        CancellationToken cancellationToken) where TJob : Job
    {
        using var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            this.lifetimeCancellationTokenSource.Token);
        await Task.Delay(delay, linkedCancellationTokenSource.Token);
        await RetryHelper.RetryOnExceptionAsync(
            creator,
            configuration.GetValue<int>("OpinionatedFramework:JobEnqueuer:MaxAttempts"),
            linkedCancellationTokenSource.Token);
    }

    private void EnsureAcceptingJobs()
    {
        lock (this.sync)
        {
            if (!this.acceptsJobs)
            {
                throw new InvalidOperationException("The job enqueuer has been stopped.");
            }
        }
    }

    private void Track(Task task)
    {
        lock (this.sync)
        {
            this.runningTasks.Add(task);
        }

        _ = task.ContinueWith(completedTask =>
        {
            lock (this.sync)
            {
                this.runningTasks.Remove(completedTask);
            }
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }
}
