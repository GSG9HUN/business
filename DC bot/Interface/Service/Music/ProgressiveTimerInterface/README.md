# Progressive Timer Interface

This folder contains the progressive timer contract used by now-playing message updates.

## Files

### IProgressiveTimerService.cs

```csharp
public interface IProgressiveTimerService
{
    Task StartAsync(IDiscordMessage message, ulong guildId);
    void Stop(ulong guildId);
}
```

## Purpose

- `StartAsync()` starts or replaces the per-guild timer that edits the now-playing message.
- `Stop()` cancels the timer for a guild.

## Related Components

- `Service/Music/ProgressiveTimer/ProgressiveTimerService.cs` - implementation
- `Service/ReactionHandler.cs` - starts the timer after sending the control message
- `Wrapper/DiscordMessageWrapper.cs` - message abstraction used for edits
