# Playlist Responses

This folder contains response DTOs for playlist endpoints.

## Files

### PlaylistSummaryResponse.cs

Reserved for compact playlist list items.

### PlaylistDetailResponse.cs

Reserved for full playlist details and track lists.

### PlaylistTrackResponse.cs

Represents one track inside a playlist.

Fields:

- `OrderNumber`
- `Title`
- `Author`
- `Duration`
- `TrackUri`

## Notes

- Playlist response DTOs should be built from persistence/service records, not EF entities.
- Keep ordering explicit so mobile clients can render stable track lists.
