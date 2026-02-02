using System;
using System.Data.Common;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config.Entities;

public class CountryCodeType : IUserType
{
    public SqlType[] SqlTypes => [SqlTypeFactory.GetString(3)];

    public Type ReturnedType => typeof(CountryCode);

    public bool IsMutable => false;

    public object? NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
        return NHibernateUtil.String.NullSafeGet(rs, names, session) is not string dbValue ? null : new CountryCode(dbValue);
    }

    public void NullSafeSet(DbCommand cmd, object? value, int index, ISessionImplementor session)
    {
        var countryCode = (CountryCode?) value;
        NHibernateUtil.String.NullSafeSet(cmd, countryCode?.IsoCode, index, session);
    }

    public object DeepCopy(object value)
    {
        return value;
    }

    public new bool Equals(object? x, object? y)
    {
        if (x == y)
        {
            return true;
        }

        if (x == null || y == null)
        {
            return false;
        }

        return x.Equals(y);
    }

    public int GetHashCode(object? x)
    {
        return x?.GetHashCode() ?? 0;
    }

    public object Replace(object original, object target, object owner) => original;

    public object Assemble(object cached, object owner) => cached;

    public object Disassemble(object value) => value;
}