using IOKode.OpinionatedFramework.ConfigureApplication;
using IOKode.OpinionatedFramework.Emailing;
using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.ContractImplementations.LoggerEmail;

public static class ServiceExtensions
{
    public static void AddLoggerEmail(this IOpinionatedServiceCollection services)
    {
        services.AddTransient<IEmailSender, LoggerEmailSender>();
    }
}