using IOKode.OpinionatedFramework.Events;

namespace IOKode.OpinionatedFramework.Tests.Events.Config;

public class Event1 : Event
{
    public int Prop1 { get; }
    public string Prop2 { get; }
    private int Prop3 { get; }
    public int Prop4 { get; }

    private Event1() {}

    public Event1(int prop1, string prop2)
    {
        Prop1 = prop1;
        Prop2 = prop2;
        Prop3 = prop1+1;
        Prop4 = prop1+2;
    }
}