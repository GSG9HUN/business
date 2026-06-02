# Progressive Timer

This folder contains the now-playing message update timer.

## Files

### ProgressiveTimerService.cs

**Implements:** `IProgressiveTimerService`

**Purpose:** Keep the now-playing Discord message updated while a Lavalink track is active.

## Behavior

- Starts one cancellable timer per guild.
- Reads the active Lavalink player and current track for the guild.
- Rebuilds the now-playing embed through `ITrackNotificationService.BuildNowPlayingEmbed()`.
- Updates the original control message with the current playback position.
- Stops and removes the guild timer when playback ends, skip/leave calls `Stop()`, or an update is cancelled.

## Notes

- Timers are tracked in a `ConcurrentDictionary<ulong, CancellationTokenSource>`.
- The timer updates every second and compensates for update duration before the next delay.
- Message update failures are logged and do not escape the background timer task.

## Related Components

- `Interface/Service/Music/ProgressiveTimerInterface/README.md`
- `Service/ReactionHandler.cs`
- `Service/Music/MusicServices/PlaybackControlService.cs`
- `Service/Music/MusicServices/TrackNotificationService.cs`
