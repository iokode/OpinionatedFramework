using System;

namespace IOKode.OpinionatedFramework.Resources.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ListResourcesAttribute(string resource, string? key = null, string? group = null) : Attribute
{
    /// <summary>
    /// The resource the operation is on, as segments separated by <c>/</c>, with the words of each segment separated by
    /// spaces. The first segment is the main resource, named in singular (<c>user</c>). Each further segment is a
    /// sub-resource of the ones before it, named as it is: an adjective that narrows them or a single element of them in
    /// singular (<c>user/active</c>, <c>user/name</c>), a collection in plural (<c>user/related users</c>).
    /// </summary>
    public string Resource { get; } = resource;
    public string? Key { get; } = key;

    /// <summary>
    /// The group the operation belongs to, or <see langword="null"/> when it belongs to none. The operations of a
    /// resource in one group are apart from those of the same resource in other groups, so the same operation on the
    /// same resource can be declared once in each group.
    /// </summary>
    public string? Group { get; } = group;
}
