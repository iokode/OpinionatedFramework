using System;

namespace IOKode.OpinionatedFramework.Resources.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CreateAttribute(string resource) : Attribute
{
    public string Resource { get; } = resource;
}