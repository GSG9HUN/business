# Messaging Exceptions

This folder contains exceptions for Discord message operation failures.

## MessageSendException

**Namespace:** `DC_bot.Exceptions.Messaging`

**Properties:**
- `Operation` (string) - The operation that failed (e.g., "SendMessage", "SendReactionControlMessage")

**When Thrown:**

### 1. Reaction Control Message Send Failure
```csharp
// In ReactionHandler.SendReactionControlMessageAsync()
catch (Exception ex)
{
    throw new MessageSendException("SendReactionControlMessage", "Failed to send reaction control message", ex);
}
```

### 2. Track Notification Send Failure
```csharp
// In TrackNotificationService.SafeSendAsync()
catch (Exception ex)
{
    throw new MessageSendException(operation, "Failed to send Discord message", ex);
}
```

## Usage in Code

### ReactionHandler.cs
- Thrown when sending reaction control messages fails
- Wraps underlying Discord API exceptions

### TrackNotificationService.cs
- Thrown when sending track notification messages fails
- Used in "now playing" messages and queue updates

## Handling

Services catch this exception to log message send failures:

```csharp
try
{
    await trackNotificationService.SendNowPlayingAsync(track);
}
catch (MessageSendException ex)
{
    logger.LogError(ex, "Failed to send track notification: {Operation}", ex.Operation);
    // Continue operation - message send is not critical
}
```

## Common Causes

- Missing channel permissions (SEND_MESSAGES, EMBED_LINKS)
- Discord API rate limiting
- Channel deleted while sending
- Network timeouts

## Related Files

- `Service/ReactionHandler.cs` - Throws for reaction messages
- `Service/Music/MusicServices/TrackNotificationService.cs` - Throws for track notifications
- `Wrapper/DiscordChannelWrapper.cs` - Channel abstraction used for sending

