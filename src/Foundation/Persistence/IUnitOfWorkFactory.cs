namespace IOKode.OpinionatedFramework.Persistence;

public interface IUnitOfWorkFactory
{
    public IUnitOfWork Create();
}