-- @migration
-- @version 3
-- @tag iokode

-- @down
drop table if exists iokode.users;

-- @up
create table iokode.users
(
    id            bigserial,
    name          text,

    primary key (id)
);