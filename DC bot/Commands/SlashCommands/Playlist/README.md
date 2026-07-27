# Playlist Slash Commands

This folder contains slash command adapters for saved playlist management.

## Command Group

`PlaylistSlashCommand.cs` exposes the `/playlist` group and delegates every confirmed action to the existing text command
pipeline through `ISlashCommandExecutor`.

## Commands

- `/playlist create name:<name>` -> `createPlaylist <name>`
- `/playlist save name:<name> url:<playlist-url>` -> `savePlaylist "<name>" <playlist-url>`
- `/playlist list` -> `listPlaylists`
- `/playlist view name:<name>` -> `viewPlaylist <name>`
- `/playlist load name:<name>` -> `loadPlaylist <name>`
- `/playlist add-song name:<name> url:<song-url-or-query>` -> `addSong "<name>" <song-url-or-query>`
- `/playlist remove-song name:<name> track-number:<number>` -> `removeSong "<name>" <number>`
- `/playlist rename current-name:<name> new-name:<name>` -> `renamePlaylist "<current-name>" "<new-name>"`
- `/playlist delete name:<name> confirm:<true>` -> `deletePlaylist <name>`

`save`, `load`, and `add-song` set `EnsureDeferredResponse` because they may wait on Lavalink track loading, voice join,
or playback startup. Commands that pass two playlist values quote playlist names before creating the text-command payload
so names with spaces are preserved by `CommandValidationService.TryParseSavePlaylistArguments`.

## Safety

- `delete` does not execute unless `confirm:true` is provided.
- Confirmation text escapes Discord mentions in the playlist name.
- Service-level validation still owns playlist name rules, limits, and persistence outcomes.

## Tests

- `UnitTests/Commands/SlashCommands/Playlist/PlaylistSlashCommandTests.cs` covers adapter delegation, quoted payloads,
  deferred flags, and delete confirmation.
- `IntegrationTests/Commands/SlashCommands/Playlist/PlaylistSlashCommandRegistrationIntegrationTests.cs` covers DI
  registration.
- `EndToEndTests/Commands/SlashCommands/Playlist/PlaylistSlashCommandEndToEndTests.cs` covers the local slash adapter ->
  executor -> text command pipeline.

## Related Components

- `Commands/TextCommands/Playlist/`
- `Service/SlashCommands/SlashCommandExecutor.cs`
- `Service/Core/CommandValidationService.cs`
- `Service/Music/PlaylistService/`
- `Interface/Service/Music/PlaylistServiceInterface/`
