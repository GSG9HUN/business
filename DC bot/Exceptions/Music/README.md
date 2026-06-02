# Music Exceptions

This folder contains exceptions for music playback operations.

## LavalinkOperationException

**Namespace:** `DC_bot.Exceptions.Music`

**Properties:**

- `Operation` (string) - The Lavalink operation that failed

**When Thrown:**

```csharp
// In LavalinkNodeConnectionService.ConnectAsync()
catch (Exception ex)
{
    throw new LavalinkOperationException("ConnectAsync", "Failed to connect to Lavalink node", ex);
}
```

**Usage:** Thrown when starting the Lavalink audio service or waiting for the node readiness signal fails.

---

## QueueOperationException

**Namespace:** `DC_bot.Exceptions.Music`

**Properties:**

- `Operation` (string) - The queue operation that failed
- `GuildId` (ulong) - Discord guild ID

**When Thrown:**

`QueueOperationException` is defined for queue persistence failure boundaries, but production code currently does not
throw it. `MusicQueueService` now lets repository exceptions bubble naturally and uses validation/logging for expected
queue states.

```csharp
throw new QueueOperationException("SetQueue", guildId, "Failed to persist queue reorder", ex);
```

**Usage:** Reserved for future queue-specific exception wrapping.

---

## TrackLoadException

**Namespace:** `DC_bot.Exceptions.Music`

**Properties:**

- `Query` (string) - The URL or search query that failed

**When Thrown:**

### 1. Track Load Failure

```csharp
// In PlaybackRequestService.PlayAsync()
catch (Exception ex)
{
    throw new TrackLoadException(query, loadFailureMessage, ex);
}
```

### 2. No Track Returned

```csharp
if (loadResult.Track is null || loadResult.IsFailed)
{
    throw new TrackLoadException(query, "Track not found or load failed");
}
```

**Usage:** Thrown when track loading/searching fails via Lavalink for URL and query requests.

---

## Usage in Code

### LavalinkNodeConnectionService.cs

- `LavalinkOperationException` - Lavalink connection failures

### PlaybackRequestService.cs

- `TrackLoadException` - Track loading from URLs or search queries

### MusicQueueService.cs

- Does not currently throw `QueueOperationException`

## Handling

Music exceptions bubble to the shared command execution boundaries. `CommandHandlerService`, `SlashCommandExecutor`, and
reaction handling catch `BotException` and log the command or operation failure.

```csharp
try
{
    await command.ExecuteAsync(message);
}
catch (BotException botEx)
{
    logger.CommandExecutionFailed(botEx, commandName);
}
```

## Related Files

- `Service/Music/LavaLinkService.cs` - Throws LavalinkOperationException and TrackLoadException
- `Service/Music/MusicServices/MusicQueueService.cs` - Throws QueueOperationException
- `Interface/Service/Persistence/IQueueRepository.cs` - Queue persistence contract

