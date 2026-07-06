# Persistence Entities

This folder contains EF Core entity classes mapped to PostgreSQL tables.

## Entities

- `GuildDataEntity` - guild-level premium state and root navigation
- `GuildPlaybackStateEntity` - repeat flags, current track identifier, and current queue item ID (`QueueItemId`)
- `GuildQueueItemEntity` - queue entries with state and timestamps
- `GuildRepeatListItemEntity` - repeat-list entries and ordering
- `GuildPremiumAuditEntity` - premium status change history table entity

## Mapping Notes

- Primary guild identifier is stored as `long` (`GuildId`).
- Queue item state values are stored as `short` but represented as `QueueItemState` in the entity and repository code.
- Navigation properties are configured in `Persistence/Configurations/`.
- `GuildPremiumAuditEntity` is part of the EF model; active repository code currently updates premium state through `GuildDataRepository`.

## Related

- `Persistence/Configurations/README.md`
- `Persistence/Migrations/README.md`
