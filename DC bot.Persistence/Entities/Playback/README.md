# Playback Entities

This folder contains playback-related EF Core entities.

## Why This Folder Exists

Playback state needs to outlive in-memory Lavalink/player state. These entities represent the durable playback snapshot and repeat-list rows that music services can restore or update.

## Files

### GuildPlaybackStateEntity.cs

Stores current playback state for one guild.

### GuildRepeatListItemEntity.cs

Stores repeat-list track identifiers and ordering for one guild.

## What The Entities Represent

`GuildPlaybackStateEntity` stores repeat flags and current track linkage. `GuildRepeatListItemEntity` stores the ordered snapshot used when repeat-list mode rehydrates the queue.

## Maintenance Notes

- Playback state stores the current queue item id when a queue item is linked.
- Repeat-list rows are separate from active queue rows.
