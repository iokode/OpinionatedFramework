using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IOKode.OpinionatedFramework.Bootstrapping;

public class OpinionatedServiceCollection : ServiceCollection, IOpinionatedServiceCollection
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