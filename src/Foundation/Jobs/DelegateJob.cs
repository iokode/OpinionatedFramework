using System;
using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Jobs;

public class DelegateJob : IJob
{
    private readonly Action _delegate;

    public DelegateJob(Action @delegate)
    {
        _delegate = @delegate;
    }
    
    public Task InvokeAsync(CancellationToken cancellationToken)
    {
        _delegate();
        return Task.CompletedTask;
    }
}