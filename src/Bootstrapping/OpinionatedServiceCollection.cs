using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IOKode.OpinionatedFramework.Bootstrapping;

/// <summary>
/// This interface extends IServiceCollection with the intention of use the typing system to enforce that
/// framework services cannot be added into another container.
/// </summary>
public interface IOpinionatedServiceCollection : IServiceCollection;

internal class OpinionatedServiceCollection : ServiceCollection, IOpinionatedServiceCollection
{
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