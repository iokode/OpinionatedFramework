using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.Commands;

[assembly: BootstrapDriver<ICommandExecutor,
    IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor.CommandExecutorBootstrapDriver>(
    "CommandExecutor", "default", true)]

namespace IOKode.OpinionatedFramework.ContractImplementations.CommandExecutor;

public sealed class CommandExecutorBootstrapDriver : IBootstrapDriverRegistrar
{
    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        return BootstrapValidationResult.Success;
    }

    public static void Register(BootstrapDriverContext context)
    {
        context.Services.AddDefaultCommandExecutor();
    }
}
