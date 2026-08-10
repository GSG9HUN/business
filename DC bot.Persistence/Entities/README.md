# Persistence Entities

This folder contains EF Core entity classes mapped to PostgreSQL tables.

## Why This Folder Exists

Entities are the persistence project's in-memory representation of database rows. Repository implementations use them to read and write data through EF Core.

They are intentionally not exposed through `DC bot.Contracts`. Service code should consume immutable contract records instead, because entity navigation properties and EF tracking behavior are implementation details.

## Subfolders

### Guilds

Guild data and premium audit entities.

### Playback

Playback state and repeat-list entities.

### Queue

Guild queue item entities.

### Playlists

Saved playlist and playlist track entities.

### MobileApps

Mobile app user, user-guild link, and refresh session entities.

### BotControl

API-to-bot command bridge entities.

## Mapping Notes

- Primary guild identifier is represented as `ulong` (`GuildId`) in entities and contracts.
- Queue item state values are stored as `short` but represented as `QueueItemState` in the entity and repository code.
- Navigation properties are configured in `../Configurations/`.
- `GuildPremiumAuditEntity` is part of the EF model; active repository code currently updates premium state through `GuildDataRepository`.
- Playlist tracks are cascade-deleted when their parent playlist is deleted.
- Mobile app refresh session entities store token hashes only.

## When To Add An Entity

Add an entity when the database needs a new table or an existing table needs new persisted columns. Also add or update the matching configuration and migration in the same change.

Do not add computed API-only or display-only fields here. Those belong in API response DTOs or service-level projections.

## Related

- `../Configurations/README.md`
- `../Migrations/README.md`
