using System.Diagnostics;
using IOKode.OpinionatedFramework.Facades;
using Microsoft.Extensions.Logging;

namespace IOKode.OpinionatedFramework.Logging;

public partial interface ILogging
{
    private ILogger FromCaller()
    {
        var callerType = new StackFrame(2).GetMethod()!.DeclaringType!;

        if (callerType == typeof(Log)) // From facade
        {
            callerType = new StackFrame(3).GetMethod()!.DeclaringType!;
        }

        // Handle the case where the caller is an async state machine
        if (callerType is { IsNested: true, DeclaringType: not null } && callerType.Name.Contains('<'))
        {
            callerType = callerType.DeclaringType;
        }

        var logger = FromCategory(callerType);

        return logger;
    }
}