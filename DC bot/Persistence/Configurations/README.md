# Persistence Configurations

This folder contains EF Core `IEntityTypeConfiguration<T>` classes.

## Purpose

Configuration classes define:

- table names and column names
- primary/foreign keys
- indexes and uniqueness constraints
- default values and required fields

## Files

- `GuildDataConfiguration.cs`
- `GuildPlaybackStateConfiguration.cs`
- `GuildQueueItemConfiguration.cs`
- `GuildRepeatListItemConfiguration.cs`
- `GuildPremiumAuditConfiguration.cs`

## Notes

- Naming convention uses snake_case table and column names.
- When changing entity shape, update configuration and add a migration.
