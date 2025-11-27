using System;
using System.Threading;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.Hangfire.Jobs;

internal class HangfireJobExecutionContext : IJobExecutionContext
{
    public required string Name { get; init; }
    public required Type JobType { get; init; }
    public required Guid TraceID { get; init; }
    public required CancellationToken CancellationToken { get; init; }
}