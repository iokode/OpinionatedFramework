using System;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations;

public class SourceQueryAttribute(Type queryType) : Attribute
{
    public Type QueryType { get; } = queryType;
}