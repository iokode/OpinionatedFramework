using System;
using IOKode.OpinionatedFramework.Ensuring;

namespace IOKode.OpinionatedFramework.Tests.Ensuring;

public class EnsuringTests
{
    [Fact]
    public void DoesNotThrowsWhenValidationPasses()
    {
        Ensure.Boolean.IsTrue(true).ElseThrows(new _CustomException());
    }

    [Fact]
    public void ThrowsWhenValidationNotPasses()
    {
        Assert.Throws<_CustomException>(() => { Ensure.Boolean.IsTrue(false).ElseThrows(new _CustomException()); });
    }

    [Fact]
    public void ThrowsArgumentNull()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            string? obj = null;
            Ensure.ArgumentNotNull(obj);
        });

        Assert.Equal("obj", exception.ParamName);
    }

    private class _CustomException : Exception
    {
    }
}