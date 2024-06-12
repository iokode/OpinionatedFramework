using Microsoft.Extensions.DependencyInjection;

namespace IOKode.OpinionatedFramework.Bootstrapping;

/// <summary>
/// This interface extends IServiceCollection with the intention of use the typing system to enforce that
/// framework services cannot be added into another container.
/// </summary>
public interface IOpinionatedServiceCollection : IServiceCollection;