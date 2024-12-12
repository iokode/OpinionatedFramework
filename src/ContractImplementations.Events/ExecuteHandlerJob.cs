using System;
using System.Threading;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Jobs;

namespace IOKode.OpinionatedFramework.ContractImplementations.Events;

// todo should use string with full name?
public class ExecuteHandlerJob(Type handlerType) : IJob
{
    public Task InvokeAsync(CancellationToken cancellationToken)
    {
        
    }
}