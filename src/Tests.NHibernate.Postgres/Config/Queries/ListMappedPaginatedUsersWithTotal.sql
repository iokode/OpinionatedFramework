-- @generate
-- @parameter string? name
-- @parameter int skip
-- @parameter int take
-- @map MappedUsersPageFilter filter -> MappedUsersPage

-- @result_set filtered_total
-- @cardinality one
-- @scalar_result int total
select count(*) as total
from users
where name = coalesce(:name, name);

-- @result_set filtered_users
-- @cardinality zero_or_more
-- @result int id
-- @result string name
select id, name
from users
where name = coalesce(:name, name)
order by id
limit :take offset :skip;
