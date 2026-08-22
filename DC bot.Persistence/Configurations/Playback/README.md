# Playback Configurations

This folder contains EF Core mappings for playback entities.

## Why This Folder Exists

Playback state has one-row-per-guild behavior, while repeat-list items are ordered child rows. Those two persistence shapes need different constraints and relationships.

Keeping their mappings together makes the playback schema easier to reason about.

## Files

### GuildPlaybackStateConfiguration.cs

Maps playback state columns and relationships.

### GuildRepeatListItemConfiguration.cs

Maps repeat-list rows and ordering constraints.

## What To Keep Here

- playback state table mapping
- repeat flag column mapping
- current track and current queue item columns
- repeat-list ordering constraints
- guild relationships for playback tables

## Maintenance Notes

- Playback state is one row per guild.
- Repeat-list items are ordered within a guild.
