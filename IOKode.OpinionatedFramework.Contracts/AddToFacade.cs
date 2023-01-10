using System;

namespace IOKode.OpinionatedFramework.Contracts;

[AttributeUsage(AttributeTargets.Interface)]
public sealed class AddToFacade : Attribute
{
    private readonly string _facadeName;

    public AddToFacade(string facadeName)
    {
        _facadeName = facadeName;
    }
}