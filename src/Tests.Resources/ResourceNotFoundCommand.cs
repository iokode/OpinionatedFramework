using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Exceptions;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[RetrieveResource("not found command", "id")]
public class ResourceNotFoundCommand(int id) : Command
{
    protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
    {
        throw new EntityNotFoundException(id);
    }
}