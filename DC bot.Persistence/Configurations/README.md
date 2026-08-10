# Persistence Configurations

This folder contains EF Core `IEntityTypeConfiguration<T>` classes grouped by feature.

## Why This Folder Exists

Entities describe data in C# terms, but configuration classes describe how that data is stored in PostgreSQL. Keeping mapping outside the entity classes prevents entity files from becoming mixed with table names, indexes, conversions, and cascade rules.

`BotDbContext.OnModelCreating` applies these configurations explicitly.

## What Configuration Classes Define

Configuration classes define:

- table names and column names
- primary and foreign keys
- indexes and uniqueness constraints
- default values and required fields
- enum/value conversions

## Subfolders

### Guilds

Guild data and premium audit table mappings.

### Playback

Playback state and repeat-list table mappings.

### Queue

Guild queue item table mappings.

### Playlists

Saved playlist and playlist track table mappings.

### MobileApps

Mobile app user, user-guild link, and refresh session table mappings.

### BotControl

API-to-bot command bridge table mappings.

### Shared

Shared EF configuration helpers.

## How To Maintain Mappings

When an entity changes, update the matching configuration in the same feature folder. If the change affects the database schema, create a migration after the configuration is updated.

Avoid relying on EF default naming for persisted schema. This project uses explicit snake_case table and column names so migrations stay predictable.

## Maintenance Notes

- Naming convention uses snake_case table and column names.
- `GuildQueueItemConfiguration` converts `QueueItemState` to the existing `short` `state` column and defaults new rows to `Queued`.
- `PlaylistConfiguration` enforces unique playlist names per guild.
- `PlaylistTrackConfiguration` enforces unique order numbers inside one playlist.
- Mobile app user IDs are Discord user IDs and are stored as the primary key.
- When changing entity shape, update configuration and add a migration.
