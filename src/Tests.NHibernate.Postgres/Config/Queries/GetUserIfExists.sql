-- @generate
-- @parameter string name
-- @cardinality zero_or_one
-- @result int id
-- @result string name

select id, name from users where name = :name;
