using System;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config;

public class TestAttribute() : Attribute
{
    public TestAttribute(string key, bool active) : this()
    {
    }
}