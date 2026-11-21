-- @generate
-- @parameter string id
-- @map -> string
-- @cardinality one
-- @result string name

select name from users where id = :id;
