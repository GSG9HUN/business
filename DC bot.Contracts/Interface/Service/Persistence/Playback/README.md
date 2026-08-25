# Playback Persistence Contracts

This folder contains playback-related persistence contracts.

## Why This Folder Exists

Playback state needs to survive process restarts and be shared across services. The bot stores repeat flags, current track identity, current queue item linkage, and repeat-list snapshots so playback behavior can be resumed consistently.

These contracts separate playback state persistence from the Lavalink/player implementation.

## Files

### IPlaybackStateRepository.cs

Contract for current playback state, repeat flags, and current queue item link.

### IRepeatListRepository.cs

Contract for storing the repeat-list snapshot used to restore repeat-list playback.

## How It Is Used

Music services update playback state when a track starts, ends, or repeat mode changes. Repeat-list services store a snapshot of track identifiers so the queue can be rehydrated when repeat-list mode loops.

## What Belongs Here

- persisted playback state
- repeat mode flags
- repeat-list snapshot persistence

## What Does Not Belong Here

- direct Lavalink calls
- Discord voice connection logic
- queue ordering and queue state transitions

## Maintenance Notes

- Playback state is guild-scoped.
- Repeat-list records are persisted separately from active queue items.
