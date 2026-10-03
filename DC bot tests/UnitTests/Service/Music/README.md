# Music Service Unit Tests

This folder contains unit tests for music playback services.

## Coverage

- `MusicQueueServiceTests.cs` verifies queue persistence boundaries and track serialization behavior.
- `LavaLinkServiceTests.cs` verifies facade delegation and queue-start orchestration.
- `PlaybackRequestServiceTests.cs` verifies play request loading, queueing, and error handling.
- `TrackPlaybackServiceTests.cs` verifies playback of loaded tracks and queued tracks.
- `TrackEndedHandlerServiceTests.cs` verifies queue progression, repeat-one, repeat-list rehydration, and load-failure handling.
- `RepeatServiceTests.cs` verifies repeat flags and repeat-list snapshot persistence.
- `CurrentTrackServiceTests.cs` verifies current-track state serialization and parsing boundaries.
- `LavalinkNodeConnectionServiceTests.cs` verifies node startup and failure mapping.
- `PlaybackEventHandlerServiceTests.cs` verifies track-ended handler registration and cleanup.
- `TrackFormatterServiceTests.cs` and `TrackNotificationServiceTests.cs` verify presentation-facing music helpers.

## Subfolders

### PlayerConnection/

Player connection tests cover voice join validation, join readiness retry, stale player cleanup, cancellation, and existing-player validation.

### PlaybackControl/

Playback control tests cover pause, resume, skip, and leave behavior.

### Playlist/

Playlist tests cover saved playlist creation, mutation, query behavior, track loading, and result mapping.

### ProgressiveTimer/

Progressive timer tests cover now-playing timer start, pause/resume, stop, and lifecycle behavior through deterministic ticker fakes.

## Run

```bash
dotnet test "DC bot tests/DC bot tests.csproj" --filter "FullyQualifiedName~Service.Music"
```
