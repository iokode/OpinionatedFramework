-- @generate
-- @parameter string? name
-- @parameter IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Address? address
-- @result string id
-- @result string name
-- @result IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Address address

SELECT *
FROM users
WHERE address = COALESCE(CAST(:address AS address_type), address)
  AND name = COALESCE(:name, name);