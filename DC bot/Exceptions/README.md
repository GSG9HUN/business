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
    ├── ValidationException (Validation/) [currently not thrown]
    └── Music Exceptions (Music/)
        ├── LavalinkOperationException
        ├── QueueOperationException [defined, currently not thrown]
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

- `ReactionControlMessageService` - Reaction control message send failures
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
- `QueueOperationException` - Defined for queue save/load boundaries, not currently thrown
- `TrackLoadException` - Track loading failures

**Thrown by:**

- `LavalinkNodeConnectionService` - Lavalink connection failures
- `PlaybackRequestService` - Track loading failures
- `MusicQueueService` - Queue persistence currently lets repository exceptions bubble naturally

---

### Validation/

Contains `ValidationException` (currently not thrown in code).

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

// Music - Lavalink node connection
try
{
    await audioService.StartAsync();
    await audioService.WaitForReadyAsync();
}
catch (Exception ex)
{
    throw new LavalinkOperationException("ConnectAsync", "Failed to connect to Lavalink node", ex);
}

// Music - Track Loading
try
{
    loadResult = await audioService.Tracks.LoadTracksAsync(query, trackSearchMode);
}
catch (Exception ex)
{
    throw new TrackLoadException(query, "Failed to load track from query", ex);
}

if (loadResult.Tracks.Count == 0)
{
    throw new TrackLoadException(query, "Track not found or load failed");
}
```

### Catching Exceptions

```csharp
// LanguageCommand catches the expected localization failure boundary.
try
{
    localizationService.SaveLanguage(message.Channel.Guild.Id, language);
}
catch (LocalizationException ex)
{
    logger.CommandExecutionFailed(ex, Name);
    await responseBuilder.SendErrorAsync(message, LocalizationKeys.LanguageCommandError);
}

// CommandHandlerService and SlashCommandExecutor catch all BotException instances.
try
{
    await command.ExecuteAsync(message);
}
catch (BotException botEx)
{
    logger.CommandExecutionFailed(botEx, commandName);
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
- Current status: defined for queue persistence failure boundaries, but not currently thrown by production code

### TrackLoadException

- `Query` (string) - URL or search query that failed

---

## Testing

### Unit Test Examples

```csharp
[Fact]
public void LoadLanguage_WhenTranslationFileIsMissing_ThrowsLocalizationException()
{
    // Arrange
    var service = new LocalizationService(logger, mockFileSystem.Object);

    // Act & Assert
    var exception = Assert.Throws<LocalizationException>(
        () => service.SaveLanguage(123UL, "invalid"));
    
    Assert.Equal("invalid", exception.LanguageCode);
    Assert.Contains("not found", exception.Message);
}

[Fact]
public async Task HandleEventAsync_WhenCommandThrowsBotException_LogsFailure()
{
    // Arrange
    commandMock
        .Setup(x => x.ExecuteAsync(It.IsAny<IDiscordMessage>()))
        .ThrowsAsync(new TrackLoadException("query", "Not found"));

    // Act
    await commandHandler.HandleEventAsync(discordClient, messageCreatedArgs);

    // Assert
    logger.VerifyCommandExecutionFailed("play");
}
```

---

## Related Components

- **Logging/LogExtensions.cs** - Exception logging extensions
- **Service/** - Services that throw domain exceptions
- **Helper/Validation/** - Validation result objects
- **Constants/AppConstants.cs** - Error message keys

---

## Best Practices

- Include relevant context in exception properties
- Preserve inner exceptions when wrapping
- Log exceptions with structured logging
- Don't expose sensitive data in exception messages

