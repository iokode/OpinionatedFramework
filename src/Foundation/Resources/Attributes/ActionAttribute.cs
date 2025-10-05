using System;

namespace IOKode.OpinionatedFramework.Resources.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ActionAttribute(string resource, string action, string? key = null) : Attribute
{
    public string Resource { get; } = resource;
    public string Action { get; } = action;
    public string? Key { get; } = key;
}