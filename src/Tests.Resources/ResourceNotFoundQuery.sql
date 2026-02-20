-- @generate
-- @using IOKode.OpinionatedFramework.Resources.Attributes
-- @attribute [RetrieveResource("not found query", "id")]
-- @parameter int id
-- @result string name
-- @single

select unnest(array['resource1', 'resource2', 'resource3']) as name
where :id = -1;