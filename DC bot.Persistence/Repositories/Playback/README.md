# Playback Repositories

This folder contains playback-related repository implementations.

## Why This Folder Exists

Playback state and repeat-list snapshots are durable music state. They need repository methods that match playback use cases instead of exposing raw table operations.

## Files

### PlaybackStateRepository.cs

Implements `IPlaybackStateRepository`.

Responsibilities:

- get or create playback state
- update repeat flags
- update current track and queue item link

### RepeatListRepository.cs

Implements `IRepeatListRepository`.

Responsibilities:

- read repeat-list track identifiers
- replace repeat-list rows transactionally
- clear repeat-list rows

## Flow Context

Music services update playback state when repeat mode changes, tracks start, or current playback changes. Repeat-list state is replaced as a snapshot so queue rehydration can be deterministic.

## Maintenance Notes

- Playback state and repeat-list persistence are guild-scoped.
- Keep repeat-list replacement transactional so partial snapshots are not stored.
