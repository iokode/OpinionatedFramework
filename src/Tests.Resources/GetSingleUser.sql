-- @generate
-- @using IOKode.OpinionatedFramework.Resources.Attributes
-- @attribute [RetrieveResource("user", "by global")]
-- @result string name
-- @single

select 'global-user' as name;