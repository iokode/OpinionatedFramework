using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.Configuration;

[assembly: BootstrapDriver<IConfigurationProvider,
    IOKode.OpinionatedFramework.ContractImplementations.MicrosoftConfiguration.MicrosoftConfigurationBootstrapDriver>(
    "Configuration", "microsoft", true)]

namespace IOKode.OpinionatedFramework.ContractImplementations.MicrosoftConfiguration;

public sealed class MicrosoftConfigurationBootstrapDriver : IBootstrapDriverRegistrar
{
    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        return BootstrapValidationResult.Success;
    }

    public static void Register(BootstrapDriverContext context)
    {
        context.Services.AddMicrosoftConfiguration(context.Configuration);
    }
}
