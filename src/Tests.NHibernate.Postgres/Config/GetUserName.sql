-- @generate
-- @using IOKode.OpinionatedFramework.Resources.Attributes
-- @attribute [Retrieve("active user", key: "name")]
-- @parameter int id
-- @result string name
-- @single

select name from users where id = :id;