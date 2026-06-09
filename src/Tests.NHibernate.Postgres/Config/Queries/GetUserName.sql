-- @generate
-- @using IOKode.OpinionatedFramework.Resources.Attributes
-- @attribute [RetrieveResource("active user", key: "name")]
-- @parameter string id
-- @cardinality one
-- @result string name

select name from users where id = :id;
