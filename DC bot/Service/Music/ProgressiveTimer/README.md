# Progressive Timer

This folder contains the now-playing message update timer.

## Files

### ProgressiveTimerService.cs

**Implements:** `IProgressiveTimerService`

**Purpose:** Keep the now-playing Discord message updated while a Lavalink track is active.

### SystemProgressTicker.cs

**Implements:** `IProgressTicker`

**Purpose:** Provide the production stopwatch/delay session used by `ProgressiveTimerService`.

## Behavior

- Starts one active timer per guild and replaces any previous active timer for the same guild.
- Reads the active Lavalink player and current track for the guild.
- Starts a fresh `IProgressTickerSession` for each timer run.
- Rebuilds the now-playing embed through `ITrackNotificationService.BuildNowPlayingEmbed()`.
- Updates the original control message with the current playback position.
- Tracks the last known position so pause/resume can continue the displayed progress instead of resetting to zero.
- Pauses timer state when `PlaybackControlService.PauseAsync()` pauses playback.
- Resumes from the stored paused state when `PlaybackControlService.ResumeAsync()` resumes the same Lavalink track.
- Stops and removes guild timer state when playback ends, skip/leave calls `Stop()`, or an update task is cancelled.

## Notes

- Active timers are tracked as per-guild timer state, not just raw cancellation tokens.
- Paused timers keep the original message, track identifier, and paused position.
- `ResumeAsync()` is a no-op when there is no paused state or the current Lavalink track no longer matches the paused track identifier.
- The production ticker updates every second and compensates for update duration before the next delay.
- Unit tests inject a fake `IProgressTicker` so timer progress can be advanced without real `Task.Delay` waits.
- Message update failures are logged and do not escape the background timer task.

## Related Components

- `Interface/Service/Music/ProgressiveTimerInterface/README.md`
- `Service/ReactionHandler/ReactionControlMessageService.cs`
- `Service/Music/MusicServices/PlaybackControlService.cs`
- `Service/Music/MusicServices/TrackEndedHandlerService.cs`
- `Service/Music/MusicServices/TrackNotificationService.cs`
