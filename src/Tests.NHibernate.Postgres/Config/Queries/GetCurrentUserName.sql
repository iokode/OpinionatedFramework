-- @generate
-- @parameter string id from middleware
-- @parameter string name
-- @cardinality zero_or_one
-- @result string name

select name from users where id = :id and name = :name;
