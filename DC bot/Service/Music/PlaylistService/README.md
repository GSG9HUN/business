# Playlist Service

This folder contains saved playlist business logic.

## Files

### PlaylistService.cs

**Implements:** `IPlaylistService`

**Purpose:** Coordinate saved playlist commands with Lavalink track loading, track serialization, and PostgreSQL repositories.

## Responsibilities

- Validate playlist names for create and rename operations.
- Create empty guild playlists.
- Save tracks loaded from a playlist URL.
- Append a single loaded song to an existing playlist.
- Remove a stored song from an existing playlist by track number.
- List saved playlists with track counts.
- View playlist tracks by deserializing stored track identifiers.
- Delete and rename saved playlists.
- Return explicit result enums instead of throwing for expected command outcomes.

## Dependencies

- `IAudioService` - Lavalink track loading.
- `IPlaylistRepository` - playlist metadata persistence.
- `IPlaylistTrackRepository` - playlist track persistence.
- `ITrackSearchResolverService` - URL/query source resolution.
- `ITrackSerializer` - Lavalink track identity serialization/deserialization.
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

## Related Components

- `Commands/TextCommands/Playlist/`
- `Interface/Service/Music/PlaylistServiceInterface/`
- `Interface/Service/Persistence/IPlaylistRepository.cs`
- `Interface/Service/Persistence/IPlaylistTrackRepository.cs`
- `Persistence/Repositories/PlaylistRepository.cs`
- `Persistence/Repositories/PlaylistTrackRepository.cs`
