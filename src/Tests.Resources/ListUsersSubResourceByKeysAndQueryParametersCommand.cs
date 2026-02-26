using System.Collections.Generic;
using System.Threading.Tasks;
using IOKode.OpinionatedFramework.Commands;
using IOKode.OpinionatedFramework.Resources.Attributes;

namespace IOKode.OpinionatedFramework.Tests.Resources;

[ListResources("users/by key", "/key1,Key2")]
public class ListUsersSubResourceByKeysAndQueryParametersCommand(string key1, int key2, int? param1, bool? param2) : Command<string[]>
{
    protected override Task<string[]> ExecuteAsync(ICommandExecutionContext executionContext)
    {
        List<string> results = [key1, key2.ToString()];

        if (param1.HasValue)
        {
            results.Add(param1.Value.ToString());
        }

        if (param2.HasValue)
        {
            results.Add(param2.Value.ToString());
        }

        return Task.FromResult(results.ToArray());
    }
}
