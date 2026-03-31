# Music Exceptions

This folder contains exceptions for music playback operations.

## LavalinkOperationException

**Namespace:** `DC_bot.Exceptions.Music`

**Properties:**

- `Operation` (string) - The Lavalink operation that failed

**When Thrown:**

```csharp
// In LavaLinkService.ConnectAsync()
catch (Exception ex)
{
    throw new LavalinkOperationException("ConnectAsync", "Failed to connect to Lavalink node", ex);
}
```

**Usage:** Thrown when connecting to the Lavalink audio server fails.

---

## QueueOperationException

**Namespace:** `DC_bot.Exceptions.Music`

**Properties:**

- `Operation` (string) - The queue operation that failed
- `GuildId` (ulong) - Discord guild ID

**When Thrown:**

### 1. Save Queue Failure

```csharp
// In MusicQueueService.SaveQueue()
catch (Exception ex)
{
    throw new QueueOperationException("SaveQueue", guildId, "Failed to save queue to file", ex);
}
```

### 2. Load Queue Failure

```csharp
// In MusicQueueService.LoadQueue()
catch (Exception ex)
{
    throw new QueueOperationException("LoadQueue", guildId, "Failed to load queue from file", ex);
}
```

**Usage:** Thrown when saving or loading queue persistence files fails.

---

## TrackLoadException

**Namespace:** `DC_bot.Exceptions.Music`

**Properties:**

- `Query` (string) - The URL or search query that failed

**When Thrown:**

### 1. Track Load from URL Failure

```csharp
// In LavaLinkService.PlayAsyncUrl()
catch (Exception ex)
{
    throw new TrackLoadException(url.ToString(), "Failed to load track from URL", ex);
}

// Or when no tracks returned
if (loadResult.Tracks.Count == 0)
{
    throw new TrackLoadException(url.ToString(), "Track not found or load failed");
}
```

### 2. Track Load from Query Failure

```csharp
// In LavaLinkService.PlayAsyncQuery()
catch (Exception ex)
{
    throw new TrackLoadException(query, "Failed to load track from query", ex);
}

// Or when no tracks returned
if (loadResult.Tracks.Count == 0)
{
    throw new TrackLoadException(query, "Track not found or load failed");
}
```

**Usage:** Thrown when track loading/searching fails via Lavalink.

---

## Usage in Code

### LavaLinkService.cs

- `LavalinkOperationException` - Lavalink connection failures
- `TrackLoadException` - Track loading from URLs or search queries

### MusicQueueService.cs

- `QueueOperationException` - Queue file save/load failures

## Handling

Commands and services catch these exceptions:

```csharp
try
{
    await lavaLinkService.PlayAsyncUrl(channel, url, message, mode);
}
catch (TrackLoadException ex)
{
    logger.LogWarning(ex, "Track not found: {Query}", ex.Query);
    await responseBuilder.SendErrorAsync(message, "track_not_found");
}
catch (LavalinkOperationException ex)
{
    logger.LogError(ex, "Lavalink error: {Operation}", ex.Operation);
    await responseBuilder.SendErrorAsync(message, "playback_error");
}
```

## Related Files

- `Service/Music/LavaLinkService.cs` - Throws LavalinkOperationException and TrackLoadException
- `Service/Music/MusicServices/MusicQueueService.cs` - Throws QueueOperationException
- `guildFiles/queues/*.json` - Queue persistence files

