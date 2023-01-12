using System;

namespace IOKode.OpinionatedFramework.Contracts;

[AttributeUsage(AttributeTargets.Interface)]
public sealed class AddToFacadeAttribute : Attribute
{
    private readonly string _facadeName;

    public AddToFacadeAttribute(string facadeName)
    {
        _facadeName = facadeName;
    }
}