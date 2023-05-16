using IOKode.OpinionatedFramework.Contracts;

namespace IOKode.OpinionatedFramework.Persistence;

/// <summary>
/// Marker interface for repositories.
/// Used in <see cref="IUnitOfWork.GetRepository{TRepository}"/> method to check at compile-time that
/// a type is a repository.
/// </summary>
public interface IRepository
{
}