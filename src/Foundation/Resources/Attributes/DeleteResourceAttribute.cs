using System;

namespace IOKode.OpinionatedFramework.Resources.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class DeleteResourceAttribute(string resource, string? key = null, string? group = null) : Attribute
{
    public string Resource { get; } = resource;
    public string? Key { get; } = key;

    /// <summary>
    /// The controller group the operation belongs to. Operations of the same resource in different groups are
    /// generated in different controllers, named <c>{Group}{Resource}ResourceController</c>, with the same routes.
    /// When null, the operation goes to <c>{Resource}ResourceController</c>.
    /// </summary>
    public string? Group { get; } = group;
}
