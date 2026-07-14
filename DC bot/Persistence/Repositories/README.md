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
- update queue item state through `QueueItemState`
- update one queue item position (`UpdateQueueItemPositionAsync`)
- mark all queued items as skipped (`MarkAllQueuedAsSkippedAsync`)
- enforce max queued items per guild
- atomically claim the next queued item (`ClaimNextQueuedItemAsync`): marks it as `playing` and returns it in a single operation
- isolate the PostgreSQL `FOR UPDATE SKIP LOCKED` claim SQL in `PostgreSqlQueueClaimSql`
- the current queue limit is 50 queued tracks per guild

### RepeatListRepository.cs

Implements `IRepeatListRepository`.

Responsibilities:

- read repeat-list track identifiers for `MusicQueueService.GetRepeatableQueue()`
- replace repeat list transactionally
- clear repeat list
- the current repeat-list limit is 50 tracks per guild

### PlaylistRepository.cs

Implements `IPlaylistRepository`.

Responsibilities:

- ensure the owning guild row exists before creating a playlist
- create saved playlist metadata
- check existence by guild and playlist name
- get a playlist by guild and name as `PlaylistRecord`
- list guild playlists with track counts as `PlaylistSummaryRecord`
- delete and rename playlists by guild and name

### PlaylistTrackRepository.cs

Implements `IPlaylistTrackRepository`.

Responsibilities:

- read playlist tracks ordered by `OrderNumber`
- append one track at the next order number
- append multiple tracks transactionally
- remove one track by order number and compact the remaining order numbers
- retry retriable PostgreSQL unique/serialization failures caused by concurrent playlist changes

## Notes

- Repositories use `IDbContextFactory<BotDbContext>` and short-lived contexts.
- Guild IDs are represented as `ulong` in repository contracts and EF entities.
- Queue item state is represented as `QueueItemState` in repository code and converted to the `short` database value by EF configuration.
