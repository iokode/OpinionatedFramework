using System;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[ListResources("users/actives/related users", "id/isActive/key")]
public class ListUsersRelatedUsersByKeysWithWrapFilterCommand(int id, string key, bool isActive, ListUsersFilter? filter = null) : Command<int[]>
{
    protected override Task<int[]> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        int[] result = [id, Convert.ToInt32(isActive), Convert.ToInt32(key), Convert.ToInt32(filter?.IsSingle ?? false)];
        return Task.FromResult(result);
    }
}

public class ListUsersFilter
{
    public bool IsSingle { get; set; }
}