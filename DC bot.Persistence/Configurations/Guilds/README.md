# Guild Configurations

This folder contains EF Core mappings for guild-level entities.

## Why This Folder Exists

Guild data is the root for most persisted features. Queue rows, playback state, repeat-list rows, playlists, and user-guild links all relate back to known guild rows.

The mappings here define the root table and premium audit table so other feature mappings can safely reference them.

## Files

### GuildDataConfiguration.cs

Maps the guild data table, premium columns, and navigation relationships.

### GuildPremiumAuditConfiguration.cs

Maps the premium audit table.

## What To Keep Here

- guild table shape
- premium state columns
- premium audit table mapping
- root navigation relationships from guild data

## Maintenance Notes

- Guild IDs are mapped consistently through shared helper configuration.
- Guild data is the parent table for many guild-scoped feature tables.
