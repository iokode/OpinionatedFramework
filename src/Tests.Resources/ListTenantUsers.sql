-- @generate
-- @using IOKode.OpinionatedFramework.Resources.Attributes
-- @attribute [ListResources("tenant user")]
-- @parameter string tenant from middleware
-- @parameter string prefix
-- @result string name

select cast(:tenant as text) || '-' || cast(:prefix as text) as name;
