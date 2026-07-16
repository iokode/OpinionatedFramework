using IOKode.OpinionatedFramework.Bootstrapping.Abstractions;
using IOKode.OpinionatedFramework.Logging;
using Microsoft.Extensions.Logging;

[assembly: BootstrapDriver<ILogging,
    IOKode.OpinionatedFramework.ContractImplementations.MicrosoftLogging.MicrosoftLoggingBootstrapDriver>(
    "Logging", "microsoft-console", true)]

namespace IOKode.OpinionatedFramework.ContractImplementations.MicrosoftLogging;

public sealed class MicrosoftLoggingBootstrapDriver : IBootstrapDriverRegistrar
{
    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        return BootstrapValidationResult.Success;
    }

    public static void Register(BootstrapDriverContext context)
    {
        context.Services.AddMicrosoftLogging(options =>
        {
            var minimumLevel = context.DriverConfiguration["MinimumLevel"] switch
            {
                "Trace" => LogLevel.Trace,
                "Debug" => LogLevel.Debug,
                "Info" or "Information" => LogLevel.Information,
                "Warn" or "Warning" => LogLevel.Warning,
                "Error" => LogLevel.Error,
                "Critical" => LogLevel.Critical,
                "None" => LogLevel.None,
                _ => LogLevel.Information
            };
            options.SetMinimumLevel(minimumLevel);
            options.AddConsole();
        });
    }
}
