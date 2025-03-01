-- @migration
-- @tag iokode

-- @up
create schema if not exists iokode;

-- @down
drop schema if exists iokode;