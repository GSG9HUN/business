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

- Empty queue → Shows error message
- No active player → Validation error

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

- Queue has 0 or 1 tracks → Error
- No active player → Validation error

---

### RepeatCommand.cs

**Command:** `!repeat`

**Description:** Toggle repeat mode for the current track.

**Behavior:**

1. Validates user
2. Calls `ILavaLinkService.Repeat()`
3. Toggles single-track repeat on/off

---

### RepeatListCommand.cs

**Command:** `!repeatList`

**Description:** Toggle repeat mode for the entire queue.

**Behavior:**

1. Validates user
2. Calls `ILavaLinkService.RepeatList()`
3. Toggles queue repeat on/off

---

### ClearCommand.cs

**Command:** `!clear`

**Description:** Clear all tracks from the queue (keeps current track playing).

**Behavior:**

1. Validates user
2. Calls `IMusicQueueService.Clear(guildId)`
3. Removes all queued tracks
4. Currently playing track continues

---

## Service Dependencies

### IMusicQueueService

- `ViewQueue(guildId)` - Read-only view
- `GetQueue(guildId)` - Mutable queue access
- `SetQueue(guildId, queue)` - Replace entire queue
- `Clear(guildId)` - Empty queue
- `Enqueue(guildId, track)` - Add track
- `Dequeue(guildId)` - Remove and return next

### ILavaLinkService

- `Repeat()` - Single track repeat
- `RepeatList()` - Queue repeat

## Queue Persistence

Queue state is persisted to:

- **Storage:** PostgreSQL via EF Core repositories
- **Contract:** `IQueueRepository`
- **Implementation:** `Persistence/Repositories/QueueRepository.cs`
- **State model:** queued/playing/played/skipped queue item lifecycle

## Related Components

- `Service/Music/MusicServices/MusicQueueService.cs` - Queue state management
- `Service/Music/MusicServices/RepeatService.cs` - Repeat mode logic
- `Interface/Service/Music/MusicServiceInterface/IMusicQueueService.cs` - Queue contract
- `Interface/Service/Persistence/IQueueRepository.cs` - Queue persistence contract
- `Persistence/Repositories/QueueRepository.cs` - Queue persistence implementation

