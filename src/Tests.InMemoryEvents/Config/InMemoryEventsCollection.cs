using Xunit;

namespace IOKode.OpinionatedFramework.Tests.InMemoryEvents.Config;

/// <summary>
/// Serializes the test classes of this project.
/// </summary>
/// <remarks>
/// They each initialize the static framework container, which one process can only have one of.
/// </remarks>
[CollectionDefinition(Name)]
public class InMemoryEventsCollection
{
    public const string Name = "in-memory events";
}
