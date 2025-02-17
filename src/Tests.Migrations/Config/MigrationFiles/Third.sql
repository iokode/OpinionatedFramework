-- @migration
-- @version 3
-- @tag iokode

-- @up
create table iokode.users
(
    id            bigserial,
    name          text,

    primary key (id)
);

-- @down
drop table if exists iokode.users;