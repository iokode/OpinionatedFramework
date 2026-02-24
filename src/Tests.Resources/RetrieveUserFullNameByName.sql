-- @generate
-- @using IOKode.OpinionatedFramework.Resources.Attributes
-- @attribute [RetrieveResource("user/name", "name")]
-- @parameter string name
-- @result string name
-- @single

select :name as name;