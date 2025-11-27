using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.Tests.Events.Config;

public class Event1 : Event
{
    public required int Prop1 { get; init; }
    public required string Prop2 { get; init; }
    private int Prop3 => Prop1 + 1;
    public int Prop4 => Prop1 + 2;
}