-- @generate
-- @parameter string name
-- @result int id
-- @result string name
-- @single_or_default

select id, name from users where name = :name;