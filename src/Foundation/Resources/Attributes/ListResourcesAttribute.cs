using System;

namespace IOKode.OpinionatedFramework.Resources.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ListResourcesAttribute(string resource, string? key = null) : Attribute
{
    public string Resource { get; } = resource;
    public string? Key { get; } = key;
}