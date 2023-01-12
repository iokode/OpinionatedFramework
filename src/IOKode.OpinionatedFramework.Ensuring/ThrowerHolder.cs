using System;

namespace IOKode.OpinionatedFramework.Ensuring;

public partial class ThrowerHolder
{
    private readonly Exception _exception;

    public ThrowerHolder(Exception exception)
    {
        _exception = exception;
    }
}