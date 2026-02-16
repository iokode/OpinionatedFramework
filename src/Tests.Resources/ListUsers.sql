-- @generate
-- @using IOKode.OpinionatedFramework.Resources.Attributes
-- @result string name

-- @attribute [ListResources("users")]

select unnest(array['user1','user2','user3']) as name;