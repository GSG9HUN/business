# Playlist Service Models

This folder contains DTOs and result enums returned by `IPlaylistService`.

## Result Enums

- `CreatePlaylistResult` - created, already exists, invalid name, or unknown error.
- `SavePlaylistResult` - saved, already exists, no tracks found, or unknown error.
- `DeletePlaylistResult` - deleted, missing playlist, or unknown error.
- `AddSongResult` - added, missing playlist, no tracks found, invalid URL, or unknown error.
- `RenamePlaylistResult` - renamed, missing playlist, duplicate name, invalid name, or unknown error.
- `RemoveSongResult` - removed, missing playlist, missing song, invalid playlist name, invalid track number, or unknown error.
- `ListPlaylistsStatus` - listed, no playlists, or unknown error.
- `ViewPlaylistStatus` - viewed, missing playlist, empty playlist, or unknown error.

## DTOs

- `PlaylistDto` - playlist name and ordered stored track DTOs.
- `PlaylistTrackDto` - stored playlist track identity data.
- `PlaylistSummaryDto` - playlist name and stored track count.
- `ListPlaylistsResult` - list status and playlist summaries.
- `PlaylistViewTrackDto` - display-ready track metadata for `viewPlaylist`.
- `ViewPlaylistResult` - view status, playlist name, and display-ready tracks.

## Notes

- DTOs are service contracts, not EF entities.
- Track identity data is serialized/deserialized through `ITrackSerializer`.
- UI/command response text is still built by commands with `ILocalizationService`.
