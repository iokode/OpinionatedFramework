-- @generate
-- @using IOKode.OpinionatedFramework.Resources.Attributes
-- @attribute [Retrieve("user", "by name")]
-- @parameter string name
-- @result string name
-- @single

select :name as name;