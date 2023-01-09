using System.Collections.Generic;
using System.Linq;

namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public class CollectionEnsurer
{
    public bool NotEmpty(IEnumerable<object> value)
    {
        if (!value.Any())
        {
            return false;
        }

        return true;
    }

    public bool Empty(IEnumerable<object> value)
    {
        return !value.Any();
    }
}