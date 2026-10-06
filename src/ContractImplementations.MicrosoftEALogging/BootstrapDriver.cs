using IOKode.OpinionatedFramework.ContractImplementations.MicrosoftEALogging;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.Logging;

[assembly: BootstrapDriver<ILogging, MicrosoftExtensionsAbstractionsLoggingBootstrapDriver>("Logging", "MicrosoftExtensions", true)]

namespace IOKode.OpinionatedFramework.ContractImplementations.MicrosoftEALogging;

public sealed class MicrosoftExtensionsAbstractionsLoggingBootstrapDriver : IBootstrapDriverRegistrar
{
    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        return BootstrapValidationResult.Success;
    }

    public static void Register(BootstrapDriverContext context)
    {
        context.Services.AddMicrosoftExtensionsAbstractionsLogging();
    }
}
