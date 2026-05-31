-- @generate
-- @using IOKode.OpinionatedFramework.Tests.NHibernate.Postgres.Config
-- @parameter string? name
-- @parameter Address? address
-- @result int id
-- @map UsersFilters? filters

select id
from users
where name = coalesce(:name, name)
  and address = coalesce(:address, address)
