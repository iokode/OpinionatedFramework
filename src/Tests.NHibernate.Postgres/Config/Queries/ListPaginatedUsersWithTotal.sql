-- @generate
-- @parameter int skip
-- @parameter int take
-- @result string name
-- @count total
-- @query_result_name UserPaginated

select name, count(*) OVER() as total from users
limit :take offset :skip;