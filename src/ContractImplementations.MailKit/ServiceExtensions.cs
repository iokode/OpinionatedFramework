using IOKode.OpinionatedFramework.Bootstrapping;
using IOKode.OpinionatedFramework.Emailing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ContractImplementations.MailKit;

public static class ServiceExtensions
{
    public static void AddMailKit(this IOpinionatedServiceCollection services, MailKitOptions options)
    {
        services.AddTransient<IEmailSender, MailKitEmailSender>(_ => new MailKitEmailSender(options));
    }

    public static void AddMailKit(this IOpinionatedServiceCollection services, IConfiguration configuration)
    {
        var options = new MailKitOptions();
        configuration.Bind(options);

        services.AddMailKit(options);
    }
}