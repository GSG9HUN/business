# Progressive Timer Interface

This folder contains the progressive timer contract used by now-playing message updates.

## Files

### IProgressiveTimerService.cs

```csharp
public interface IProgressiveTimerService
{
    Task StartAsync(IDiscordMessage message, ulong guildId);
    Task ResumeAsync(ulong guildId);
    void Pause(ulong guildId);
    void Stop(ulong guildId);
}
```

## Purpose

- `StartAsync()` starts or replaces the per-guild timer that edits the now-playing message.
- `Pause()` captures timer state for a guild and cancels the active timer without discarding the paused position.
- `ResumeAsync()` restarts a paused guild timer when the same Lavalink track is still active.
- `Stop()` cancels active timer state and discards paused timer state for a guild.

## Related Components

- `Service/Music/ProgressiveTimer/ProgressiveTimerService.cs` - implementation
- `Service/ReactionHandler/ReactionControlMessageService.cs` - starts the timer after sending the control message
- `Service/Music/MusicServices/PlaybackControlService.cs` - pauses, resumes, and stops timer state for playback controls
- `Wrapper/DiscordMessageWrapper.cs` - message abstraction used for edits
