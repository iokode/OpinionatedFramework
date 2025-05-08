-- @generate
-- @using IOKode.OpinionatedFramework.Tests.NHibernate.Postgres
-- @parameter string? user_name
-- @parameter Address? address
-- @result string id
-- @result string user_name
-- @result Address address

SELECT id, name as user_name, address
FROM users
WHERE address = COALESCE(CAST(:address AS address_type), address)
  AND name = COALESCE(:user_name, name);