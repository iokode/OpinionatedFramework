using System;
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

public class GeneratedCollectionEnsurer
{
    // todo Remove this class
    private readonly Exception _exception;

    public GeneratedCollectionEnsurer(Exception exception)
    {
        _exception = exception;
    }

    public void NotEmpty(IEnumerable<object> value)
    {
        bool ok = CollectionEnsurer.NotEmpty(value);

        if (!ok)
        {
            throw _exception;
        }
    }

    public void Empty(IEnumerable<object> value)
    {
        bool ok = CollectionEnsurer.Empty(value);

        if (!ok)
        {
            throw _exception;
        }
    }
}