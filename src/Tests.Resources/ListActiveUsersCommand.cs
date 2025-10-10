using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[List("user", "by actives")]
public class ListActiveUsersCommand(ListUsersFilter? filter = null) : Command<int[]>
{
    protected override Task<int[]> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        var result = (int[]) (filter?.IsSingle ?? false ? [1] : [1, 2, 3]);
        return Task.FromResult(result);
    }
}

public class ListUsersFilter
{
    public bool IsSingle { get; set; }
}