-- @generate
-- @parameter string name
-- @cardinality zero_or_one
-- @result int id
-- @map -> bool

select id from users where name = :name;
