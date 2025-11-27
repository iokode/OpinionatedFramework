using System;
using System.Data;
using System.Data.Common;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Npgsql;

namespace IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config;

public sealed class AddressType : IUserType
{
    private static readonly SqlType[] sqlTypes = [new(DbType.Object)];

    public bool IsMutable => false;

    public Type ReturnedType => typeof(Address);

    public SqlType[] SqlTypes => sqlTypes;

    public new bool Equals(object? x, object? y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;
        return x.Equals(y);
    }

    public int GetHashCode(object x) => x?.GetHashCode() ?? 0;

    public object DeepCopy(object value) => value;

    public object Disassemble(object value) => value;
    public object Assemble(object cached, object owner) => cached;

    public object Replace(object original, object target, object owner) => original;

    public object? NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
        var index = rs.GetOrdinal(names[0]);
        if (rs.IsDBNull(index))
        {
            return null;
        }

        // NpgsqlConnection.GlobalTypeMapper.MapComposite<(string, string, string)>("address_type");
        var tuple = ((NHybridDataReader) rs).Target.GetFieldValue<AddressDto>(index);

        return new Address
        (
            line: tuple.Line,
            region: tuple.Region,
            countryCode: new CountryCode(tuple.CountryCode)
        );
    }

    public void NullSafeSet(DbCommand cmd, object? value, int index, ISessionImplementor session)
    {
        var parameter = (NpgsqlParameter) cmd.Parameters[index];

        if (value == null)
        {
            parameter.Value = DBNull.Value;
        }
        else
        {
            var address = (Address) value;
            parameter.Value = new AddressDto
            {
                Line = address.Line,
                Region = address.Region,
                CountryCode = address.CountryCode.IsoCode
            };
            parameter.DataTypeName = "address_type";
            //parameter.NpgsqlDbType = NpgsqlDbType.;
        }
    }
}

class AddressDto
{
    public required string Line { get; init; }
    public required string Region { get; init; }
    public required string CountryCode { get; init; }
}