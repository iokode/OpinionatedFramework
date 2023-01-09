namespace IOKode.OpinionatedFramework.Contracts.Persistence;

[Contract]
public interface IUnitOfWorkFactory
{
    public IUnitOfWork Create();
}