using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IOKode.OpinionatedFramework.ServiceContainer;

/// <summary>
/// This interface extends IServiceCollection with the intention of use the typing system to enforce that
/// framework services cannot be added into another container.
/// </summary>
public interface IOpinionatedServiceCollection : IServiceCollection;

internal class OpinionatedServiceCollection : ServiceCollection, IOpinionatedServiceCollection
{
    /// <summary>
    /// Copies the service descriptors to a new collection.
    /// </summary>
    /// <remarks>
    /// The new collection remains mutable until it is assigned to a container and initialized.
    /// </remarks>
    /// <param name="services">The original collection.</param>
    /// <returns>A new collection containing the same service descriptors.</returns>
    public static OpinionatedServiceCollection Copy(IServiceCollection services)
    {
        var collection = new OpinionatedServiceCollection();
        foreach (var descriptor in services)
        {
            collection.Add(descriptor);
        }

        return collection;
    }
}
