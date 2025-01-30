using System;
using System.Data;
using System.Data.Common;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Npgsql;
using NpgsqlTypes;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres.Mappings;

public class JsonBType : IUserType
{
    public SqlType[] SqlTypes => new[] { new SqlType(DbType.Object) };
    public Type ReturnedType => typeof(string);
    public bool IsMutable => false;

    public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
    {
        ((NpgsqlParameter) cmd.Parameters[index]).NpgsqlDbType = NpgsqlDbType.Jsonb;
        NHibernateUtil.MetaType.NullSafeSet(cmd, value, index, session);
    }
    
    public object? NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
        var value = rs[names[0]];
        return value.ToString();
    }

    public new bool Equals(object x, object y)
    {
        return x.Equals(y);
    }

    public object DeepCopy(object value) => value;

    public int GetHashCode(object x) => x.GetHashCode();

    public object Replace(object original, object target, object owner) => original;

    public object Assemble(object cached, object owner) => cached;

    public object Disassemble(object value) => value;
}
