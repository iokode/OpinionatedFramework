using System;
using System.Threading;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.TaskRunJobs;

public class JobContext : IJobExecutionContext
{
    public string Name { get; set; }
    public Type JobType { get; set; }
    public Guid TraceID { get; set; }
    public CancellationToken CancellationToken { get; set; }
}