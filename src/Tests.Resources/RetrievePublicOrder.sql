-- @generate
-- @using IOKode.OpinionatedFramework.Resources.Attributes
-- @attribute [RetrieveResource("order", "id", group: "Public")]
-- @parameter int id
-- @cardinality one
-- @result string name

select 'public-order-' || cast(:id as text) as name;
