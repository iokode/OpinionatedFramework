using System;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[ListResources("users/actives/related users", "id/isActive/key")]
public class ListUsersRelatedUsersByKeysWithWrapFilterCommand : Command<int[]>
{
    public required int Id { get; init; }
    public required string Key { get; init; }
    public required bool IsActive { get; init; }
    public ListUsersFilter? Filter { get; init; }

    protected override Task<int[]> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        int[] result = [Id, Convert.ToInt32(IsActive), Convert.ToInt32(Key), Convert.ToInt32(Filter?.IsSingle ?? false)];
        return Task.FromResult(result);
    }
}

public class ListUsersFilter
{
    public bool IsSingle { get; set; }
}
