# Playback Models

This folder contains playback persistence records.

## Why This Folder Exists

Playback services need a compact snapshot of persisted playback state without depending on the EF entity that maps the database table.

The model here represents what service code needs to know, not how the table is configured.

## Files

### PlaybackStateRecord.cs

Immutable record for guild playback state.

## Flow Context

When the bot loads or changes playback state, repositories return this record to describe repeat flags, current track identity, and the optional queue item link.

## Maintenance Notes

- `QueueItemId` links the current playback state to a persisted queue item when available.
- Add fields here only when service code genuinely needs them across the repository boundary.
