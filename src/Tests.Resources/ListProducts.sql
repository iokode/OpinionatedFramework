-- @generate
-- @using IOKode.OpinionatedFramework.Resources.Attributes
-- @result string name

-- @attribute [ListResources("product")]

select unnest(array['product1','product2','product3']) as name;