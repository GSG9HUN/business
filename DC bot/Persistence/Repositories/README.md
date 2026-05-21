# Persistence Repositories

This folder contains repository implementations for persistence contracts.

## Repositories

### GuildDataRepository.cs

Implements `IGuildDataRepository`.

Responsibilities:

- ensure guild row exists
- query effective premium status, including optional expiry
- upsert premium state values
- no dedicated premium audit write path currently exists in this repository

### PlaybackStateRepository.cs

Implements `IPlaybackStateRepository`.

Responsibilities:

- get/create playback state per guild
- update repeat flags
- update current track identifier and linked queue item ID (`queueItemId`)

### QueueRepository.cs

Implements `IQueueRepository`.

Responsibilities:

- get queued items in playback order
- check whether queued items exist (`AnyQueuedItemsAsync`)
- get the next queued item without changing state (`GetNextQueuedItemAsync`)
- get the previous played/skipped item (`GetPreviousItemAsync`)
- enqueue and query queued items
- enqueue multiple tracks (`EnqueueManyAsync`)
- reorder queue items transactionally
- update queue item state (`queued`, `playing`, `played`, `skipped`)
- update one queue item position (`UpdateQueueItemPositionAsync`)
- mark all queued items as skipped (`MarkAllQueuedAsSkippedAsync`)
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
