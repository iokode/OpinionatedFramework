using System;
using NodaTime;

namespace IOKode.OpinionatedFramework.ContractImplementations.NHibernate.Postgres.UserTypes.NodaTime;

public class InstantUserType : ValueObjectConverterUserType<Instant, DateTime>
{
    public override string DataTypeName => "timestamptz";
    public override Instant FromProvider(DateTime provider) => Instant.FromDateTimeUtc(provider);
    public override DateTime ToProvider(Instant valueObject) => valueObject.ToDateTimeUtc();
}