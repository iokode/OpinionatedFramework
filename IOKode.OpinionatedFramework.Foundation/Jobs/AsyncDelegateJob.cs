using System;
using System.Threading;
using System.Threading.Tasks;

namespace IOKode.OpinionatedFramework.Foundation.Jobs;

public class AsyncDelegateJob : IJob
{
    private readonly Func<Task> _delegate;

    public AsyncDelegateJob(Func<Task> @delegate)
    {
        _delegate = @delegate;
    }
    
    public async Task InvokeAsync(CancellationToken cancellationToken)
    {
        await _delegate();
    }
}