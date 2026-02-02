-- @generate
-- @using IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config
-- @parameter string? name
-- @parameter Address? address
-- @abstract UsersFilters? filters -> int
-- @count

select count(*) as "count"
from users
where name = coalesce(:name, name)
and address = coalesce(:address, address)