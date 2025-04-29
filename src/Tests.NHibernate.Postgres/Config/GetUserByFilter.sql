-- @generate
-- @using IOKode.OpinionatedFramework.Tests.NHibernate.Postgres
-- @parameter string? name
-- @parameter Address? address
-- @result string id
-- @result string name
-- @result Address address

SELECT *
FROM users
WHERE address = COALESCE(CAST(:address AS address_type), address)
  AND name = COALESCE(:name, name);