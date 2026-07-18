using System.Collections.Generic;
using IOKode.OpinionatedFramework.Drivers.Abstractions;
using IOKode.OpinionatedFramework.Emailing;

[assembly: BootstrapDriver<IEmailSender,
    IOKode.OpinionatedFramework.ContractImplementations.MailKit.MailKitBootstrapDriver>("Email", "mailkit")]

namespace IOKode.OpinionatedFramework.ContractImplementations.MailKit;

public sealed class MailKitBootstrapDriver : IBootstrapDriverRegistrar
{
    public static BootstrapValidationResult Validate(BootstrapDriverContext context)
    {
        var errors = new List<BootstrapValidationError>();
        if (string.IsNullOrWhiteSpace(context.DriverConfiguration["Host"]))
        {
            errors.Add(new BootstrapValidationError(
                $"{context.DriverConfiguration.Path}:Host",
                "A value is required."));
        }

        if (!int.TryParse(context.DriverConfiguration["Port"], out var port) || port <= 0)
        {
            errors.Add(new BootstrapValidationError(
                $"{context.DriverConfiguration.Path}:Port",
                "The value must be a positive integer."));
        }

        return new BootstrapValidationResult(errors);
    }

    public static void Register(BootstrapDriverContext context)
    {
        context.Services.AddMailKit(context.DriverConfiguration);
    }
}
