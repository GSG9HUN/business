# Custom Exception Handling Implementation

## Overview
This document describes the custom exception handling system implemented in the Discord bot application.

## Custom Exception Hierarchy

### Base Exception
- **`BotException`**: Abstract base class for all bot-related exceptions
  - Location: `DC bot/Exceptions/BotException.cs`
  - All custom exceptions inherit from this base class

### Specific Exception Types

1. **`LavalinkOperationException`**
   - Used for: Lavalink audio service operation failures
   - Properties: `Operation` (string)
   - Usage: Connection failures, track loading errors, playback issues

2. **`TrackLoadException`**
   - Used for: Track loading and playback failures
   - Properties: `Query` (string)
   - Usage: Track not found, invalid URLs, search failures

3. **`MessageSendException`**
   - Used for: Discord message sending failures
   - Properties: `Operation` (string)
   - Usage: Channel message sending errors, reaction control messages

4. **`QueueOperationException`**
   - Used for: Music queue operation failures
   - Properties: `Operation` (string), `GuildId` (ulong)
   - Usage: Queue save/load errors, serialization failures

5. **`ValidationException`**
   - Used for: Validation failures
   - Properties: `ValidationKey` (string)
   - Usage: User validation, connection validation errors

6. **`LocalizationException`**
   - Used for: Localization service failures
   - Properties: `LanguageCode` (string)
   - Usage: Translation file not found, JSON read/write errors

## Implementation Details

### Services with Exception Handling

#### LavaLinkService
- **Operations covered**:
  - `ConnectAsync()`: Throws `LavalinkOperationException` on connection failure
  - `PlayAsyncUrl()`: Throws `TrackLoadException` on track load failure
  - `PlayAsyncQuery()`: Throws `TrackLoadException` on search failure
  - `SafeSendAsync()`: Throws `MessageSendException` on message send failure
  
#### LocalizationService
- **Operations covered**:
  - `ReadJson<T>()`: Throws `LocalizationException` on file read failure
  - `WriteJson<T>()`: Throws `LocalizationException` on file write failure
  - `LoadTranslations()`: Throws `LocalizationException` when translation file not found

#### MusicQueueService
- **Operations covered**:
  - `SaveQueue()`: Throws `QueueOperationException` on save failure
  - `LoadQueue()`: Throws `QueueOperationException` on load failure

#### CommandHandlerService
- **Operations covered**:
  - `ExecuteAsync()`: Catches and re-throws `BotException` and general exceptions
  - Differentiates between custom bot exceptions and unexpected errors

#### ReactionHandler
- **Operations covered**:
  - `SendReactionControlMessage()`: Throws `MessageSendException` on failure
  - `OnReactionAdded()`: Catches `BotException` and general exceptions
  - `OnReactionRemoved()`: Catches `BotException` and general exceptions

## Logging Integration

All custom exceptions are integrated with structured logging:

### New Log Events
- **EventId 1208**: `ReactionHandlerOperationFailed` - Reaction handler operation errors
- **EventId 1209**: `ReactionHandlerMessageSendFailed` - Reaction message send errors

### Existing Log Events Enhanced
- All exception catch blocks now log with appropriate context
- Custom exceptions preserve inner exception details for debugging

## Benefits

1. **Type Safety**: Strongly typed exceptions for different failure scenarios
2. **Contextual Information**: Each exception carries relevant context (operation name, guild ID, query, etc.)
3. **Better Debugging**: Inner exceptions preserved with full stack traces
4. **Centralized Handling**: Consistent exception handling across services
5. **Structured Logging**: Integration with existing logging infrastructure

## Usage Example

```csharp
try
{
    await audioService.Tracks.LoadTracksAsync(url.ToString(), trackSearchMode);
}
catch (Exception ex)
{
    logger.LavalinkOperationFailed(ex, "LoadTracksAsyncUrl");
    throw new TrackLoadException(url.ToString(), "Failed to load track from URL", ex);
}
```

## Testing Recommendations

1. Unit tests should verify custom exceptions are thrown in failure scenarios
2. Integration tests should verify exception handling doesn't break service operations
3. Log output should be verified to ensure exceptions are properly logged

## Future Enhancements

1. Add retry logic for transient failures (e.g., network errors)
2. Implement circuit breaker pattern for Lavalink connection failures
3. Add telemetry/metrics for exception occurrences
4. Consider adding user-friendly error messages based on exception type

