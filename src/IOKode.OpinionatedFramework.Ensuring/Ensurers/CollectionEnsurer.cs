using System.Collections.Generic;
using System.Linq;

namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public static class CollectionEnsurer
{
    public static bool NotEmpty(IEnumerable<object> value)
    {
        if (!value.Any())
        {
            return false;
        }

        return true;
    }

    public static bool Empty(IEnumerable<object> value)
    {
        return !value.Any();
    }
}