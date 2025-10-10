-- @generate
-- @using IOKode.OpinionatedFramework.Resources.Attributes
-- @attribute [Retrieve("user", "by global")]
-- @result string name
-- @single

select 'global-user' as name;