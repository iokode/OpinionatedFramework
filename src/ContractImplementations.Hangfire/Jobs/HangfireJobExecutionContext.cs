using System;
using System.Threading;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;

internal class HangfireJobExecutionContext : IJobExecutionContext
{
    public string Name { get; set; }
    public Type JobType { get; set; }
    public Guid TraceID { get; set; }
    public CancellationToken CancellationToken { get; set; }
}