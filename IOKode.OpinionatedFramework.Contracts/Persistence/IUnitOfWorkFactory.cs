namespace IOKode.OpinionatedFramework.Contracts.Persistence;

public interface IUnitOfWorkFactory
{
    public IUnitOfWork Create();
}