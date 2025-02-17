-- @migration
-- @version 2
-- @namespace IOKode.OpinionatedFramework.Tests.Migrations.Config.MigrationFiles

-- @up
create table iokode.products
(
    id            bigserial,
    name          text,

    primary key (id)
);

-- @down
drop table if exists iokode.products;