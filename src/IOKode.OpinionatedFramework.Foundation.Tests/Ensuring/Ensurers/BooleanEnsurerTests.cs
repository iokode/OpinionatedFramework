using IOKode.OpinionatedFramework.Ensuring.Ensurers;

namespace IOKode.OpinionatedFramework.Foundation.Tests.Ensuring.Ensurers;

public class BooleanEnsurerTests
{
    [Fact]
    public void IsTrue_WithTrueValue()
    {
        bool result = BooleanEnsurer.IsTrue(true);
        Assert.True(result);
    }

    [Fact]
    public void IsTrue_WithFalseValue()
    {
        bool result = BooleanEnsurer.IsTrue(false);
        Assert.False(result);
    }

    [Fact]
    public void IsFalse_WithTrueValue()
    {
        bool result = BooleanEnsurer.IsFalse(true);
        Assert.False(result);
    }

    [Fact]
    public void IsFalse_WithFalseValue()
    {
        bool result = BooleanEnsurer.IsFalse(false);
        Assert.True(result);
    }
}