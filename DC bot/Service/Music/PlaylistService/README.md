# Playlist Service

This folder contains saved playlist business logic.

## Files

### PlaylistService.cs

**Implements:** `IPlaylistService`

**Purpose:** Coordinate saved playlist commands with Lavalink track loading, track serialization, and PostgreSQL repositories.

## Responsibilities

- Normalize and validate playlist names for every public playlist entrypoint.
- Create empty guild playlists.
- Save tracks loaded from a playlist URL.
- Append the first loaded song to an existing playlist, including Lavalink single-track fallback results.
- Remove a stored song from an existing playlist by track number.
- List saved playlists with track counts.
- View playlist tracks by deserializing stored track identifiers.
- Delete and rename saved playlists.
- Enforce saved playlist limits from `PlaylistOptions`.
- Clean up a newly-created playlist if saving tracks fails after playlist creation.
- Return explicit result enums instead of throwing for expected command outcomes.

## Dependencies

- `IAudioService` - Lavalink track loading.
- `IPlaylistRepository` - playlist metadata persistence.
- `IPlaylistTrackRepository` - playlist track persistence.
- `ITrackSearchResolverService` - URL/query source resolution.
- `ITrackSerializer` - Lavalink track identity serialization/deserialization.
- `IOptions<PlaylistOptions>` - saved playlist count, track count, and import limits.
- `ILogger<PlaylistService>` - structured logging.

## Result Models

Public service methods return DTOs and result enums from `Interface/Service/Music/PlaylistServiceInterface/Models/`.

Examples:

- `CreatePlaylistResult`
- `SavePlaylistResult`
- `AddSongResult`
- `RemoveSongResult`
- `RenamePlaylistResult`
- `ListPlaylistsResult`
- `ViewPlaylistResult`

## Persistence Boundary

The service consumes repository records from `Interface/Service/Persistence/Models/`, not EF Core entities. EF entities stay inside `Persistence/`.

`SavePlaylistAsync` serializes loaded track identities before creating the playlist record. If the playlist record is
created but `IPlaylistTrackRepository.AddRangeAsync` fails, the service attempts to delete the newly-created playlist and
then returns `SavePlaylistResult.UnknownError`.

## Limits

`PlaylistOptions` defaults:

- `MaxPlaylistsPerGuild = 50`
- `MaxTracksPerPlaylist = 250`
- `MaxImportedTracks = 100`

The service clamps configured values to at least `1` before applying them.

## Related Components

- `Commands/TextCommands/Playlist/`
- `Commands/SlashCommands/Playlist/`
- `Configuration/PlaylistOptions.cs`
- `Interface/Service/Music/PlaylistServiceInterface/`
- `Interface/Service/Persistence/IPlaylistRepository.cs`
- `Interface/Service/Persistence/IPlaylistTrackRepository.cs`
- `Persistence/Repositories/PlaylistRepository.cs`
- `Persistence/Repositories/PlaylistTrackRepository.cs`
