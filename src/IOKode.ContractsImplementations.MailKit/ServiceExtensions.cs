using IOKode.OpinionatedFramework.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.ContractsImplementations.MailKit;

public static class ServiceExtensions
{
    public static void AddMailKit(this IServiceCollection services, MailKitOptions options)
    {
        services.AddTransient<IEmailSender, EmailSender>(_ =>
            new EmailSender(options.SmtpHost, options.SmtpPort, options.UserName, options.Password));
    }

    public static void AddMailKit(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new MailKitOptions();
        configuration.Bind(options);

        services.AddMailKit(options);
    }
}