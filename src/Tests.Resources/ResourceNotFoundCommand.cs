using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Persistence.UnitOfWork.QueryBuilder.Exceptions;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[RetrieveResource("not found command", "id")]
public class ResourceNotFoundCommand : Command
{
    public required int Id { get; init; }

    protected override Task ExecuteAsync(ICommandExecutionContext executionContext)
    {
        throw new EntityNotFoundException(Id);
    }
}
