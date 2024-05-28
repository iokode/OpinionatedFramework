using System;
using IOKode.OpinionatedFramework.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IOKode.OpinionatedFramework.ContractImplementations.MicrosoftLogging;

public static class ServiceExtensions
{
    public static void AddMicrosoftLogging(this IServiceCollection services, Action<ILoggingBuilder> builder)
    {
        services.AddLogging();
        services.AddSingleton<ILogging>(_ =>
        {
            var factory = LoggerFactory.Create(builder);
            var logging = new Logging(factory);
            return logging;
        });
    }
}