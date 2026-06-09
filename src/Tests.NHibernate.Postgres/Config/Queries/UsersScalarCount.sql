-- @generate
-- @cardinality one
-- @scalar_result int count

select count(*) as "count"
from users
