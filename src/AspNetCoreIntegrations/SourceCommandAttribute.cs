using System;

namespace IOKode.OpinionatedFramework.AspNetCoreIntegrations;

public class SourceCommandAttribute(Type commandType) : Attribute
{
    public Type CommandType { get; } = commandType;
}

public class SourceCommandAttribute<TCommandType>() : SourceCommandAttribute(typeof(TCommandType)) where TCommandType : class;