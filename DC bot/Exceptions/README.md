# Exceptions

This folder contains custom exception types for domain-specific error handling.

## Overview

All custom exceptions inherit from `BotException`, providing:

- Type-safe error handling
- Domain-specific context data
- Clear error boundaries

## Exception Hierarchy

```
System.Exception
    ↓
BotException (abstract)
    ├── LocalizationException (Localization/)
    ├── MessageSendException (Messaging/)
    ├── ValidationException (Validation/) [Not currently used]
    └── Music Exceptions (Music/)
        ├── LavalinkOperationException
        ├── QueueOperationException
        └── TrackLoadException
```

## Base Exception

### BotException.cs

**Purpose:** Abstract base class for all bot-related exceptions.

```csharp
public abstract class BotException : Exception
{
    protected BotException(string message) : base(message) { }
    
    protected BotException(string message, Exception innerException) 
        : base(message, innerException) { }
}
```

---

## Subfolders

### Localization/

Contains `LocalizationException` for language file loading errors.

**Thrown by:**

- `LocalizationService` - File read/write failures, translation file not found

**Common causes:**

- Missing language file
- Invalid JSON in language file
- File I/O errors

---

### Messaging/

Contains `MessageSendException` for Discord message operation failures.

**Thrown by:**

- `ReactionHandler` - Reaction message send failures
- `TrackNotificationService` - Track notification send failures

**Common causes:**

- Missing channel permissions
- Discord API rate limiting
- Channel deleted during send

---

### Music/

Contains music playback exceptions.

**Exception types:**

- `LavalinkOperationException` - Lavalink connection failures
- `QueueOperationException` - Queue save/load failures
- `TrackLoadException` - Track loading failures

**Thrown by:**

- `LavaLinkService` - Lavalink operations and track loading
- `MusicQueueService` - Queue persistence

---

### Validation/

Contains `ValidationException` (currently not used in code).

The application uses validation result objects instead (`UserValidationResult`, `PlayerValidationResult`,
`ConnectionValidationResult`).

---

## Usage Example

### Throwing Exceptions

```csharp
// Localization
try
{
    var json = _fileSystem.ReadAllText(filePath);
    return JsonSerializer.Deserialize<T>(json);
}
catch (Exception ex)
{
    throw new LocalizationException(_lang ?? "unknown", $"Failed to read JSON file: {filePath}", ex);
}

// Messaging
try
{
    await channel.SendMessageAsync(embed);
}
catch (Exception ex)
{
    throw new MessageSendException("SendReactionControlMessage", "Failed to send reaction control message", ex);
}

// Music - Lavalink
try
{
    await _audioService.ConnectAsync(configuration);
}
catch (Exception ex)
{
    throw new LavalinkOperationException("ConnectAsync", "Failed to connect to Lavalink node", ex);
}

// Music - Queue
try
{
    await queueRepository.ReorderQueuedItemsAsync(guildId, reorderedTrackIdentifiers);
}
catch (Exception ex)
{
    throw new QueueOperationException("SetQueue", guildId, "Failed to persist queue reorder", ex);
}

// Music - Track Loading
if (loadResult.Tracks.Count == 0)
{
    throw new TrackLoadException(url.ToString(), "Track not found or load failed");
}
```

### Catching Exceptions

```csharp
// Catch specific exception
try
{
    await lavaLinkService.PlayAsyncUrl(channel, url, message, mode);
}
catch (TrackLoadException ex)
{
    logger.LogWarning(ex, "Track not found: {Query}", ex.Query);
    await responseBuilder.SendErrorAsync(message, "track_not_found");
}

// Catch all bot exceptions
try
{
    await command.ExecuteAsync(message);
}
catch (BotException ex)
{
    logger.LogError(ex, "Bot exception in command: {CommandName}", command.Name);
    await responseBuilder.SendErrorAsync(message, "command_failed");
}
```

---

## Exception Properties

### LocalizationException

- `LanguageCode` (string) - Language code that caused the error

### MessageSendException

- `Operation` (string) - Operation that failed

### LavalinkOperationException

- `Operation` (string) - Lavalink operation that failed

### QueueOperationException

- `Operation` (string) - Queue operation that failed
- `GuildId` (ulong) - Discord guild ID

### TrackLoadException

- `Query` (string) - URL or search query that failed

---

## Testing

### Unit Test Examples

```csharp
[Fact]
public async Task LoadLanguageAsync_FileNotFound_ThrowsLocalizationException()
{
    // Arrange
    var service = new LocalizationService(mockFileSystem.Object);

    // Act & Assert
    var exception = await Assert.ThrowsAsync<LocalizationException>(
        () => service.LoadLanguageAsync("invalid")
    );
    
    Assert.Equal("invalid", exception.LanguageCode);
    Assert.Contains("not found", exception.Message);
}

[Fact]
public async Task ExecuteAsync_TrackLoadException_SendsErrorMessage()
{
    // Arrange
    mockLavaLink
        .Setup(x => x.PlayAsync(It.IsAny<string>()))
        .ThrowsAsync(new TrackLoadException("query", "Not found"));

    // Act
    await command.ExecuteAsync(message);

    // Assert
    mockResponseBuilder.Verify(
        x => x.SendErrorAsync(message, "track_not_found"),
        Times.Once
    );
}
```

---

## Related Components

- **Logging/LogExtensions.cs** - Exception logging extensions
- **Service/** - Services that throw domain exceptions
- **Helper/Validation/** - Validation result objects
- **Constants/LocalizationKeys.cs** - Error message keys

---

## Best Practices

- Include relevant context in exception properties
- Preserve inner exceptions when wrapping
- Log exceptions with structured logging
- Don't expose sensitive data in exception messages

