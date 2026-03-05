# Music Services

This subfolder contains specialized music-related services.

## What's here?

Music-specific services that manage:
- Queue operations and persistence
- Repeat mode state
- Current track tracking
- Track notifications and messaging

These services are extracted from `LavaLinkService` to follow the **Single Responsibility Principle**.

## Contents

### MusicQueueService.cs
**Purpose:** Manage music queues per guild with file persistence.

**Key Responsibilities:**
- Enqueue tracks
- Dequeue tracks
- View queue contents
- Persist queue to disk
- Load queue from disk
- Clone queue for repeat list
- Initialize guild queues

**Key Methods:**
```csharp
public interface IMusicQueueService
{
    void Enqueue(ulong guildId, ILavaLinkTrack track);
    LavalinkTrack? Dequeue(ulong guildId);
    bool HasTracks(ulong guildId);
    IReadOnlyCollection<ILavaLinkTrack> ViewQueue(ulong guildId);
    void Clone(ulong guildId, LavalinkTrack currentTrack);
    IEnumerable<ILavaLinkTrack> GetRepeatableQueue(ulong guildId);
    Task LoadQueue(ulong guildId);
    void Init(ulong guildId);
}
```

**Persistence:**
- Queue is saved to `guildFiles/queues/{guildId}.json`
- Automatically loaded on bot restart
- Serialized as JSON array of tracks

**Usage:**
```csharp
// Add to queue
musicQueueService.Enqueue(guildId, track);

// Get next track
var nextTrack = musicQueueService.Dequeue(guildId);

// View queue (read-only)
var queue = musicQueueService.ViewQueue(guildId);

// Clone for repeat list
musicQueueService.Clone(guildId, currentTrack);
```

**File Format:**
```json
[
  {
    "Identifier": "test Identifier"
  }
]
```

### RepeatService.cs
**Purpose:** Manage repeat mode state per guild.

**Key Responsibilities:**
- Track repeat mode (single track)
- Track repeat list mode (entire queue)
- Initialize guild state
- Provide dictionary access for backward compatibility

**Key Methods:**
```csharp
public class RepeatService
{
    void SetRepeating(ulong guildId, bool isRepeating);
    bool IsRepeating(ulong guildId);
    
    void SetRepeatingList(ulong guildId, bool isRepeatingList);
    bool IsRepeatingList(ulong guildId);
    
    void Init(ulong guildId);
    
    Dictionary<ulong, bool> IsRepeatingDictionary { get; }
    Dictionary<ulong, bool> IsRepeatingListDictionary { get; }
}
```

**Repeat Modes:**
1. **Single Track Repeat** - Replay current track indefinitely
2. **List Repeat** - Replay entire queue when empty
3. **No Repeat** (default) - Stop when queue is empty

**Usage:**
```csharp
// Enable repeat for current track
repeatService.SetRepeating(guildId, true);

// Check if repeating
if (repeatService.IsRepeating(guildId))
{
    await player.PlayAsync(currentTrack); // Replay
}

// Enable list repeat
repeatService.SetRepeatingList(guildId, true);
```

**Behavior:**
```
Track Ends
  ↓
Is Single Repeat? → YES → Replay current track
  ↓ NO
Queue Has Tracks? → YES → Play next track
  ↓ NO
Is List Repeat? → YES → Clone queue, play first track
  ↓ NO
Queue Empty → Stop
```

### CurrentTrackService.cs
**Purpose:** Track which song is currently playing per guild.

**Key Responsibilities:**
- Store current track reference
- Retrieve current track
- Format track info for display
- Initialize guild state

**Key Methods:**
```csharp
public class CurrentTrackService
{
    void SetCurrentTrack(ulong guildId, LavalinkTrack track);
    LavalinkTrack? GetCurrentTrack(ulong guildId);
    bool TryGetCurrentTrack(ulong guildId, out LavalinkTrack? track);
    string GetCurrentTrackFormatted(ulong guildId);
    void Init(ulong guildId);
}
```

**Usage:**
```csharp
// Set current track when playing
currentTrackService.SetCurrentTrack(guildId, track);

// Get current track
var current = currentTrackService.GetCurrentTrack(guildId);

// Get formatted string
var formatted = currentTrackService.GetCurrentTrackFormatted(guildId);
// Returns: "Artist - Title" or "No track currently playing"

// Safe retrieval
if (currentTrackService.TryGetCurrentTrack(guildId, out var track))
{
    // Track exists
}
```

**Why Separate Service?**
- Lavalink player state can be unreliable
- Need consistent state even after player disconnect
- Support "now playing" display in multiple places
- Enable repeat mode (needs to know what was playing)

### TrackNotificationService.cs
**Purpose:** Send track-related notifications to Discord channels.

**Key Responsibilities:**
- Send "now playing" messages
- Send "queue empty" notifications
- Trigger track started events
- Safe message sending (with error handling)
- Prevent spam (rate limiting)

**Key Methods:**
```csharp
public class TrackNotificationService
{
    Task NotifyNowPlayingAsync(IDiscordChannel channel, ILavaLinkTrack track);
    Task NotifyQueueEmptyAsync(IDiscordChannel channel);
    Task SendSafeAsync(IDiscordChannel channel, string message, string context);
    Task TrackStartedEventTrigger(IDiscordChannel channel, DiscordClient client, ILavaLinkTrack track);
    
    event Func<IDiscordChannel, DiscordClient, string, Task> TrackStarted;
}
```

**Message Types:**
1. **Now Playing** - "🎵 Now playing: Artist - Title"
2. **Queue Empty** - "Queue is empty. Add more tracks!"
3. **Track Added** - "Added to queue: Artist - Title"
4. **Playlist Added** - "Playlist added to queue (X tracks)"

**Usage:**
```csharp
// Notify when track starts
await trackNotificationService.NotifyNowPlayingAsync(textChannel, track);

// Notify when queue is empty
await trackNotificationService.NotifyQueueEmptyAsync(textChannel);

// Safe send (won't throw on failure)
await trackNotificationService.SendSafeAsync(channel, "Message", "Context");

// Subscribe to track started event
trackNotificationService.TrackStarted += async (channel, client, message) =>
{
    // Do something when track starts (e.g., add reactions)
};
```

**Error Handling:**
- Catches Discord API errors
- Logs failures with context
- Doesn't throw (prevents crashing on notification failures)
- Rate limit aware

**Event Integration:**
```
Track Starts
  ↓
LavaLinkService → TrackNotificationService.TrackStartedEventTrigger()
  ↓
TrackStarted Event Fires
  ↓
ReactionHandler.SendReactionControlMessage()
  ↓
Adds emoji reactions (▶️ ⏸️ ⏭️)
```

## Service Interaction

```
┌─────────────────────────────────────────┐
│         LavaLinkService                 │
│  (Orchestrates music playback)          │
└──────────┬──────────────────────────────┘
           │
           ├─→ MusicQueueService
           │   ├─ Enqueue(track)
           │   ├─ Dequeue() → next track
           │   └─ ViewQueue() → display
           │
           ├─→ RepeatService
           │   ├─ IsRepeating()
           │   └─ IsRepeatingList()
           │
           ├─→ CurrentTrackService
           │   ├─ SetCurrentTrack()
           │   └─ GetCurrentTrack()
           │
           └─→ TrackNotificationService
               ├─ NotifyNowPlayingAsync()
               └─ NotifyQueueEmptyAsync()
```

## Per-Guild State

All services manage per-guild state:

```csharp
// Initialize on guild join
musicQueueService.Init(guildId);
repeatService.Init(guildId);
currentTrackService.Init(guildId);

// Use throughout lifecycle
musicQueueService.Enqueue(guildId, track);
repeatService.SetRepeating(guildId, true);
currentTrackService.SetCurrentTrack(guildId, track);

// Cleanup on guild leave
// (No explicit cleanup needed - state remains for next join)
```

## Thread Safety

Services use thread-safe collections:
- `ConcurrentDictionary<ulong, Queue<Track>>` for queues
- `ConcurrentDictionary<ulong, bool>` for repeat flags
- Lock-based access for file I/O

## Testing

Music services are unit tested:

```csharp
[Fact]
public void Enqueue_AddsTrackToQueue()
{
    // Arrange
    var service = new MusicQueueService(mockFileSystem, logger);
    service.Init(guildId);

    // Act
    service.Enqueue(guildId, track);

    // Assert
    Assert.True(service.HasTracks(guildId));
}

[Fact]
public void SetRepeating_EnablesRepeat()
{
    // Arrange
    var service = new RepeatService();
    service.Init(guildId);

    // Act
    service.SetRepeating(guildId, true);

    // Assert
    Assert.True(service.IsRepeating(guildId));
}
```

## Best Practices

- ✅ Always call `Init(guildId)` before using services
- ✅ Use `HasTracks()` before `Dequeue()`
- ✅ Handle `Dequeue()` returning null (queue empty)
- ✅ Use `ViewQueue()` for read-only access (don't modify)
- ✅ Use `TrackNotificationService.SendSafeAsync()` for non-critical messages
- ❌ Don't modify queue returned by `ViewQueue()` (it's immutable)
- ❌ Don't forget to save queue after modifications
- ❌ Don't assume state exists (always check or initialize)

## Related

- **Service/LavaLinkService.cs** - Orchestrator service
- **Interface/IMusicQueueService.cs** - Queue service contract
- **Helper/SerializedTrack.cs** - Queue serialization DTO
- **IO/PhysicalFileSystem.cs** - File system abstraction

