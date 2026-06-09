-- @generate
-- @using IOKode.OpinionatedFramework.Resources.Attributes
-- @attribute [RetrieveResource("user/name", "name")]
-- @parameter string name
-- @cardinality one
-- @result string name

select :name as name;
