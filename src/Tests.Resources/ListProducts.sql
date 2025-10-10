-- @generate
-- @using IOKode.OpinionatedFramework.Resources.Attributes
-- @attribute [List("product")]
-- @result string name

select unnest(array['product1','product2','product3']) as name;