using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.ConfigureApplication;
using IOKode.OpinionatedFramework.ContractImplementations.Hangfire;
using IOKode.OpinionatedFramework.Jobs;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IOKode.OpinionatedFramework.Tests.Hangfire;

public class JobsTest
{
    [Fact]
    public async Task EnqueueJob()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddUserSecrets<JobsTest>()
            .AddEnvironmentVariables()
            .Build();
        Container.Services.AddHangfire(configuration);
        Container.Initialize();
        var jobEnqueuer = Locator.Resolve<IJobEnqueuer>();
        await jobEnqueuer.EnqueueAsync(Queue.Default, new Job(), default);
        Assert.True(true);
    }
}

public class Job : IJob
{
    public Task InvokeAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}