using System;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations;

public class SourceCommandAttribute(string commandType) : Attribute
{
    public string CommandTypeString { get; } = commandType;

    public Type? CommandType { get; } = Type.GetType(commandType);
}