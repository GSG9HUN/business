# Queue Commands

This folder contains text commands for managing the music playback queue.

## Commands

### ViewQueueCommand.cs

**Command:** `!viewList`

**Description:** Display the current music queue.

**Behavior:**

1. Validates user
2. Retrieves queue via `IMusicQueueService.ViewQueue(guildId)`
3. Creates Discord embed with first 10 tracks
4. Shows footer if queue has more than 10 tracks

**Error Cases:**

- Empty queue -> warning response

---

### ShuffleCommand.cs

**Command:** `!shuffle`

**Description:** Randomly shuffle the current queue order.

**Behavior:**

1. Validates user
2. Retrieves queue via `IMusicQueueService.GetQueue(guildId)`
3. Applies Fisher-Yates shuffle algorithm
4. Sets shuffled queue back via `IMusicQueueService.SetQueue(guildId, shuffledQueue)`

**Implementation:**

```csharp
// Fisher-Yates algorithm with retry to ensure order changes
for (var i = trackList.Count - 1; i > 0; i--)
{
    var j = random.Next(i + 1);
    (trackList[i], trackList[j]) = (trackList[j], trackList[i]);
}
```

**Error Cases:**

- Queue has 0 or 1 tracks -> warning response

---

### RepeatCommand.cs

**Command:** `!repeat`

**Description:** Toggle repeat mode for the current track.

**Behavior:**

1. Validates user
2. Checks repeat-list state through `IRepeatService.IsRepeatingListAsync(guildId)`
3. Toggles single-track repeat through `IRepeatService.SetRepeatingAsync(guildId, value)`
4. Uses `ICurrentTrackService.GetCurrentTrackFormattedAsync(guildId)` for the enabled response

---

### RepeatListCommand.cs

**Command:** `!repeatList`

**Description:** Toggle repeat mode for the entire queue.

**Behavior:**

1. Validates user
2. Checks single-track repeat state through `IRepeatService.IsRepeatingAsync(guildId)`
3. When enabling, snapshots the current track plus queued tracks through `IRepeatService.SaveRepeatListSnapshotAsync(...)`
4. Toggles list repeat through `IRepeatService.SetRepeatingListAsync(guildId, value)`
5. When playback reaches the end and repeat-list mode is still enabled, `TrackEndedHandlerService` reloads that snapshot through `IMusicQueueService.GetRepeatableQueue()` and copies it back into queue storage through `EnqueueMany()`

---

### ClearCommand.cs

**Command:** `!clear`

**Description:** Clear all tracks from the queue (keeps current track playing).

**Behavior:**

1. Validates user
2. Calls `IMusicQueueService.ClearQueue(guildId)`
3. Removes all queued tracks
4. Currently playing track continues

---

## Service Dependencies

### IMusicQueueService

- `ViewQueue(guildId)` - Read-only view
- `GetQueue(guildId)` - Mutable queue access
- `SetQueue(guildId, queue)` - Replace entire queue
- `ClearQueue(guildId)` - Empty queue
- `Enqueue(guildId, track)` - Add track
- `EnqueueMany(guildId, tracks)` - Bulk add tracks, used when rehydrating repeat-list playback
- `Dequeue(guildId)` - Remove and return next
- `GetRepeatableQueue(guildId)` - Read the saved repeat-list snapshot and return parseable tracks

### IRepeatService

- `IsRepeatingAsync(guildId)` / `SetRepeatingAsync(guildId, value)` - Single-track repeat state
- `IsRepeatingListAsync(guildId)` / `SetRepeatingListAsync(guildId, value)` - Queue repeat-list state
- `SaveRepeatListSnapshotAsync(guildId, currentTrack, queuedTracks)` - Persist the repeat-list snapshot

### ICurrentTrackService / ITrackFormatterService

- `GetCurrentTrackFormattedAsync(guildId)` - Format the single-track repeat response
- `FormatCurrentTrackListAsync(guildId)` - Format repeat-list enable/disable responses

## Queue Persistence

Queue state is persisted to:

- **Storage:** PostgreSQL via EF Core repositories
- **Contract:** `IQueueRepository`
- **Implementation:** `Persistence/Repositories/QueueRepository.cs`
- **Claim workflow:** `Persistence/Repositories/QueueClaimService.cs` is used internally by `QueueRepository.ClaimNextQueuedItemAsync`
- **State model:** `QueueItemState` queued/playing/played/skipped queue item lifecycle

Repeat-list snapshots are stored separately through `IRepeatListRepository`; they are read by `MusicQueueService.GetRepeatableQueue()` and copied back into the queue through `IQueueRepository.EnqueueManyAsync`.

## Related Components

- `Service/Music/MusicServices/MusicQueueService.cs` - Queue state management
- `Service/Music/MusicServices/RepeatService.cs` - Repeat flags and repeat-list snapshot writes
- `Interface/Service/Music/IMusicQueueService.cs` - Queue contract
- `Interface/Service/Persistence/IQueueRepository.cs` - Queue persistence contract
- `Interface/Service/Persistence/IRepeatListRepository.cs` - Repeat-list snapshot persistence contract
- `Persistence/Repositories/QueueRepository.cs` - Queue persistence implementation
- `Persistence/Repositories/QueueClaimService.cs` - Queue claim transaction helper

