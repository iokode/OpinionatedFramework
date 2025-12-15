-- @generate
-- @using IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config
-- @parameter string? name
-- @parameter Address? address
-- @result int id
-- @abstract CancellationToken cancellationToken, UsersFilters? filters = null

select id
from users
where name = coalesce(:name, name)
  and address = coalesce(:address, address)