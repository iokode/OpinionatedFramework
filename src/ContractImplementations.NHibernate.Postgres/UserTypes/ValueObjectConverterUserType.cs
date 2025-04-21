using System;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.UserTypes;
using Npgsql;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres.UserTypes;

/// <summary>
/// NHibernate <see cref="IUserType"/> base class that persists a domain
/// value object (<typeparamref name="TValueObject"/>) by converting it to an
/// intermediate *provider* representation (<typeparamref name="TProvider"/>)
/// understood by the underlying database driver.
///
/// The provider instance is expected to be immutable (or at least treated as
/// such) because NHibernate will cache and compare it.
/// The domain object itself can be mutable if your model requires it.
/// </summary>
/// <typeparam name="TValueObject">
///   The type your domain code works with (e.g. <c>Address</c>,
///   <c>EmailAddress</c>).
/// </typeparam>
/// <typeparam name="TProvider">
///   The exact CLR type that is written to / read from the database
///   (e.g. <c>AddressProvider</c>, <see langword="string"/>, <c>Guid</c>, etc.).
/// </typeparam>
/// <remarks>
/// Deriving classes must implement:
/// <list type="bullet">
///   <item><description>
///     <see cref="FromProvider"/> – builds the domain object from the
///     provider value returned by the data reader.
///   </description></item>
///   <item><description>
///     <see cref="ToProvider"/> – converts the domain object to the provider
///     value that will be sent to the database.
///   </description></item>
/// </list>
/// All null‑handling, deep‑copying, equality, and immutability semantics are
/// inherited from <see cref="InmutableNativeValueUserType{TValue}"/>.
/// </remarks>
public abstract class ValueObjectConverterUserType<TValueObject, TProvider> : InmutableNativeValueUserType<TValueObject> 
{
    public abstract TValueObject? FromProvider(TProvider provider);
    public abstract TProvider ToProvider(TValueObject valueObject);

    public override object? NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
        var dto = (TProvider) rs[names[0]];
        return FromProvider(dto);
    }

    public override void NullSafeSet(DbCommand cmd, object? value, int index, ISessionImplementor session)
    {
        var parameter = (NpgsqlParameter) cmd.Parameters[index];

        if (value == null)
        {
            parameter.Value = DBNull.Value;
        }
        else
        {
            var valueObject = (TValueObject) value;
            parameter.Value = ToProvider(valueObject);
            parameter.DataTypeName = DataTypeName;
        }
    }
}