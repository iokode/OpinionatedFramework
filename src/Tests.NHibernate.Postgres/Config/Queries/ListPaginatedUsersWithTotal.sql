-- @generate
-- @custom_global_directive
-- @parameter int skip
-- @parameter int take

-- @result_set total
-- @custom_total_directive
-- @cardinality one
-- @scalar_result int total
select count(*) as total from users;

-- @result_set users
-- @custom_users_directive
-- @cardinality zero_or_more
-- @result string name
select name from users
limit :take offset :skip;
