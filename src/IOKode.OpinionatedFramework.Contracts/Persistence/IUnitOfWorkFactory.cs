namespace IOKode.OpinionatedFramework.Contracts;

public interface IUnitOfWorkFactory
{
    public IUnitOfWork Create();
}