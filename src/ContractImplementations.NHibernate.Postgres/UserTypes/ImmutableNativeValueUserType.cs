using System;
using System.Data;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Npgsql;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres.UserTypes;

/// <summary>
/// Base <see cref="IUserType"/> for database columns whose contents can be
/// stored and retrieved **as‑is** with an immutable CLR value type
/// (<typeparamref name="TValue"/>).
/// Typical use‑case: PostgreSQL <c>enum</c> or <c>domain</c> types that are
/// semantically richer than plain text in the database but are already
/// represented perfectly by a <c>string</c>, <c>Guid</c>, <c>decimal</c>, etc.
/// </summary>
/// <typeparam name="TValue">
/// CLR type that the database value maps to directly (e.g. <c>string</c>).
/// </typeparam>
/// <remarks>
/// "Native" hints that the value travels between the database and .NET
/// unchanged—no parsing, composing, or extra logic— just a straight‑through,
/// 1‑to‑1 mapping of a database value that already has a natural CLR representation.
/// 
/// Sub‑classes only need to supply the concrete database type.
/// <see cref="DataTypeName"/> (e.g. <c>core.customer_discriminator</c>).
/// </remarks>
public abstract class ImmutableNativeValueUserType<TValue> : IUserType
{
    public SqlType[] SqlTypes => [new (DbType.Object)];
    public Type ReturnedType => typeof(TValue);
    public bool IsMutable => false;
    public abstract string DataTypeName { get; }

    public virtual object? NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
        var value = (TValue) rs[names[0]];
        return value;
    }

    public virtual void NullSafeSet(DbCommand cmd, object? value, int index, ISessionImplementor session)
    {
        var parameter = (NpgsqlParameter) cmd.Parameters[index];

        if (value == null)
        {
            parameter.Value = DBNull.Value;
        }
        else
        {
            parameter.Value = (TValue) value;
            parameter.DataTypeName = DataTypeName;
        }
    }

    public new bool Equals(object? x, object? y)
    {
        if (x == null ^ y == null)
        {
            return false;
        }
        
        return x == null || x.Equals(y);
    }

    public object DeepCopy(object value) => value;

    public int GetHashCode(object x) => x.GetHashCode();

    public object Replace(object original, object target, object owner) => original;

    public object Assemble(object cached, object owner) => cached;

    public object Disassemble(object value) => value;
}