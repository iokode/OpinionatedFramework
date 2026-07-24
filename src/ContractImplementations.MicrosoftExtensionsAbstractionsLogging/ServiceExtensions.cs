using IOKode.OpinionatedFramework.Logging;
using IOKode.OpinionatedFramework.ServiceContainer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IOKode.OpinionatedFramework.ContractImplementations.MicrosoftExtensionsAbstractionsLogging;

public static class ServiceExtensions
{
    /// <summary>
    /// Bridges the framework's <see cref="ILogging"/> onto the <see cref="ILoggerFactory"/> the host registered.
    /// </summary>
    /// <remarks>
    /// The driver implements no logging of its own. It resolves the ambient factory, so <c>Log</c> and
    /// <see cref="ILogger{T}"/> share one pipeline and one configuration, and any backend implementing the
    /// Microsoft.Extensions.Logging abstractions backs the framework's logging. The host must register an
    /// <see cref="ILoggerFactory"/>; resolving <see cref="ILogging"/> without one throws.
    /// </remarks>
    public static void AddMicrosoftExtensionsAbstractionsLogging(this IOpinionatedServiceCollection services)
    {
        services.AddSingleton<ILogging>(serviceProvider => new Logging(serviceProvider.GetRequiredService<ILoggerFactory>()));
    }
}
