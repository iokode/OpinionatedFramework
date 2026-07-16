using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Bootstrapping.Abstractions;
using IOKode.OpinionatedFramework.Emailing;

[assembly: BootstrapDriver<IEmailSender,
    IOKode.OpinionatedFramework.ContractImplementations.LoggerEmail.LoggerEmailBootstrapDriver>(
    "Email", "logger", true)]

namespace IOKode.OpinionatedFramework.ContractImplementations.LoggerEmail;

public sealed class LoggerEmailBootstrapDriver : IBootstrapDriverRegistrar
{
    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        return BootstrapValidationResult.Success;
    }

    public static void Register(BootstrapDriverContext context)
    {
        context.Services.AddLoggerEmail();
    }
}
