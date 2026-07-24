using IOKode.OpinionatedFramework.ContractImplementations.MicrosoftExtensionsAbstractionsLogging;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.Logging;

[assembly: BootstrapDriver<ILogging, MicrosoftExtensionsAbstractionsLoggingBootstrapDriver>("Logging", "microsoft-extensions-abstractions", true)]

namespace IOKode.OpinionatedFramework.ContractImplementations.MicrosoftExtensionsAbstractionsLogging;

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
