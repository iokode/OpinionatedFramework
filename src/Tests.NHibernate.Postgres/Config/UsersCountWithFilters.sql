-- @generate
-- @using IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config
-- @parameter string? name
-- @parameter Address? address
-- @result int count
-- @abstract UsersFilters? filters -> int
-- @single

select count(*) as "count"
from users
where name = coalesce(:name, name)
and address = coalesce(:address, address)