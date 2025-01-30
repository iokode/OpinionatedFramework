-- @generate
-- @parameter int id
-- @result string name
-- @single

select name from users where id = :id;