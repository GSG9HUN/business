# Queue Responses

This folder contains response DTOs for queue endpoints.

## Files

### QueueResponse.cs

Represents a guild queue snapshot.

Fields:

- `GuildId`
- `TrackCount`
- `Tracks`

### QueueTrackResponse.cs

Represents one queued track.

Fields:

- `Position`
- `Title`
- `Author`
- `Duration`
- `TrackUri`
- `ArtworkUri`

## Notes

- Queue responses should preserve playback order.
- Queue write endpoints should return updated snapshots or accepted command results depending on whether work is immediate or delegated to the bot process.
