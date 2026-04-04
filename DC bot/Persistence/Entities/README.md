# Persistence Entities

This folder contains EF Core entity classes mapped to PostgreSQL tables.

## Entities

- `GuildDataEntity` - guild-level premium state and root navigation
- `GuildPlaybackStateEntity` - repeat flags and current track identifier
- `GuildQueueItemEntity` - queue entries with state and timestamps
- `GuildRepeatListItemEntity` - repeat-list entries and ordering
- `GuildPremiumAuditEntity` - premium status change history

## Mapping Notes

- Primary guild identifier is stored as `long` (`GuildId`).
- Queue item state values are numeric (`short`) and interpreted by repository logic.
- Navigation properties are configured in `Persistence/Configurations/`.

## Related

- `Persistence/Configurations/README.md`
- `Persistence/Migrations/README.md`
