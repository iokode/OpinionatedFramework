-- @migration
-- @tag opinionated_framework

-- @up
CREATE TABLE IF NOT EXISTS opinionated_framework.events
(
    id            UUID PRIMARY KEY,
    event_type    TEXT  NOT NULL,
    dispatched_at TIMESTAMP,
    payload       JSONB NOT NULL
);

-- @down
DROP TABLE opinionated_framework.events;