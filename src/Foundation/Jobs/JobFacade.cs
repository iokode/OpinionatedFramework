using System;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.Facades;

public static partial class Job
{
    public static IJob Create<TJob>(JobArguments<TJob>? jobArguments = null) where TJob : IJob
    {
        var job = jobArguments == null ? Activator.CreateInstance<TJob>() : jobArguments.CreateJob();
        return job;
    }
}