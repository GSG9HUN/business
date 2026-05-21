# Persistence Repositories

This folder contains repository implementations for persistence contracts.

## Repositories

### GuildDataRepository.cs

Implements `IGuildDataRepository`.

Responsibilities:

- ensure guild row exists
- query effective premium status
- upsert premium state values

### PlaybackStateRepository.cs

Implements `IPlaybackStateRepository`.

Responsibilities:

- get/create playback state per guild
- update repeat flags
- update current track identifier and linked queue item ID (`queueItemId`)

### QueueRepository.cs

Implements `IQueueRepository`.

Responsibilities:

- enqueue and query queued items
- reorder queue items transactionally
- update queue item state (`queued`, `playing`, `played`, `skipped`)
- enforce max queued items per guild
- atomically claim the next queued item (`ClaimNextQueuedItemAsync`): marks it as `playing` and returns it in a single operation

### RepeatListRepository.cs

Implements `IRepeatListRepository`.

Responsibilities:

- read repeat-list track identifiers
- replace repeat list transactionally
- clear repeat list

## Notes

- Repositories use `IDbContextFactory<BotDbContext>` and short-lived contexts.
- Guild IDs are converted between `ulong` (domain) and `long` (database).
