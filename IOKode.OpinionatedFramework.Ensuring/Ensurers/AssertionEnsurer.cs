namespace IOKode.OpinionatedFramework.Ensuring.Ensurers;

[Ensurer]
public class AssertionEnsurer
{
    public bool Assert(bool condition)
    {
        return condition;
    }
}