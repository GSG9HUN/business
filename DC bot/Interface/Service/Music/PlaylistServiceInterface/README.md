# PlaylistServiceInterface

This folder contains the saved playlist service contract.

## Files

- `IPlaylistService.cs` - saved playlist use-case interface
- `Models/` - result enums and DTOs returned by playlist service methods

## Interface

`IPlaylistService` exposes guild-scoped saved playlist operations:

- `CreatePlaylistAsync`
- `SavePlaylistAsync`
- `LoadPlaylistAsync`
- `ListPlaylistsAsync`
- `ViewPlaylistAsync`
- `DeletePlaylistAsync`
- `AddSongToPlaylistAsync`
- `RemoveSongFromPlaylistAsync`
- `RenamePlaylistAsync`

## Design

- Uses `ulong` guild IDs to match Discord domain objects.
- Returns explicit result enums for command-level branching.
- Exposes invalid-name and limit statuses so commands can respond without parsing exceptions.
- Returns DTOs for service-facing read models.
- Removes playlist tracks by stored order number so command users can use `viewPlaylist` output directly.
- Keeps EF Core entities out of command and service contracts.

## Implementation

- `Service/Music/PlaylistService/PlaylistService.cs`
- `Configuration/PlaylistOptions.cs`

## Related Components

- `Commands/TextCommands/Playlist/`
- `Commands/SlashCommands/Playlist/`
- `Interface/Service/Music/PlaylistServiceInterface/Models/`
- `Interface/Service/Persistence/IPlaylistRepository.cs`
- `Interface/Service/Persistence/IPlaylistTrackRepository.cs`
