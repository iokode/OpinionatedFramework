-- @generate
-- @using IOKode.OpinionatedFramework.Resources.Attributes
-- @attribute [RetrieveResource("not found query", "id")]
-- @parameter int id
-- @cardinality one
-- @result string name

select unnest(array['resource1', 'resource2', 'resource3']) as name
where :id = -1;
