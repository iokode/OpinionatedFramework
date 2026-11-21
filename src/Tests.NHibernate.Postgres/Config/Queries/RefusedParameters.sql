-- @generate
-- @map string? refused -> int
-- @parameter string? name
-- @cardinality one
-- @result int count

select count(*) as "count" from users where name = coalesce(:name, name);
