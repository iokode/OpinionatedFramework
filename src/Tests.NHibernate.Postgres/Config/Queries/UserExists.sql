-- @generate
-- @parameter string name
-- @result int id
-- @map -> bool
-- @single_or_default

select id from users where name = :name;
