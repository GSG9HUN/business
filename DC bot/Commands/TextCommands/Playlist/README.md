# Playlist Commands

This folder contains text commands for saved playlist management.

Playlist names are normalized and validated by the playlist service for every service entrypoint. Text commands that
need two values support quoted playlist names, for example `!addSong "road trip" madeon imperium`.

## Commands

### CreatePlaylistCommand.cs

**Command:** `!createPlaylist <playlistName>`

**Description:** Create an empty saved playlist for the current guild.

**Behavior:**

1. Validates the user.
2. Reads the playlist name through `ICommandHelper.TryGetArgumentAsync`.
3. Calls `IPlaylistService.CreatePlaylistAsync(guildId, playlistName)`.
4. Sends success, warning, or error responses based on `CreatePlaylistResult`.

**Error Cases:**

- Playlist already exists -> warning.
- Playlist name is empty, too long, or contains line breaks -> warning.
- Guild playlist limit reached -> warning.
- Unexpected service failure -> error.

---

### SavePlaylistCommand.cs

**Command:** `!savePlaylist <playlistName> <playlistUrl>`

**Description:** Load a playlist URL through Lavalink and save its tracks.

**Behavior:**

1. Validates the user.
2. Parses playlist name and URL through `ICommandHelper.TryParseSavePlaylistArguments`.
3. Calls `IPlaylistService.SavePlaylistAsync(guildId, playlistName, playlistUrl)`.
4. Stores serialized track identifiers through the playlist service and repositories.

**Error Cases:**

- Playlist already exists -> warning.
- No tracks were found from the provided URL -> warning.
- Playlist name is invalid -> warning.
- Guild playlist limit reached -> warning.
- Loaded playlist exceeds the configured import or per-playlist track limit -> warning.
- Unexpected service failure -> error.

The service serializes loaded tracks before creating the playlist and deletes the just-created playlist if track insert
fails, so an `UnknownError` does not leave an empty saved playlist behind.

---

### DeletePlaylistCommand.cs

**Command:** `!deletePlaylist <playlistName>`

**Description:** Delete a saved playlist and its stored tracks.

**Behavior:**

1. Validates the user.
2. Reads the playlist name.
3. Calls `IPlaylistService.DeletePlaylistAsync(guildId, playlistName)`.
4. Sends a localized response for deleted, missing, invalid-name, or unknown-error outcomes.

---

### AddSongToPlaylistCommand.cs

**Command:** `!addSong <playlistName> <songUrlOrQuery>`

**Description:** Load a single song URL or search query and append the first loaded track to an existing saved playlist.

**Behavior:**

1. Validates the user.
2. Parses playlist name and song URL.
3. Calls `IPlaylistService.AddSongToPlaylistAsync(guildId, playlistName, songUrl)`.
4. Appends the track at the next playlist order number.

**Error Cases:**

- Playlist does not exist -> warning.
- URL or query cannot be loaded -> warning.
- No tracks found -> warning.
- Playlist name is invalid -> warning.
- Playlist track limit reached -> warning.
- Unexpected service failure -> error.

The add-song path handles Lavalink single-track fallback results the same way as save, so a valid single track does not
produce a false `NoTracksFound` response.

---

### ListPlaylistsCommand.cs

**Command:** `!listPlaylists`

**Description:** List saved playlists for the current guild.

**Behavior:**

1. Validates the user.
2. Calls `IPlaylistService.ListPlaylistsAsync(guildId)`.
3. Displays playlist names with stored track counts.

---

### ViewPlaylistCommand.cs

**Command:** `!viewPlaylist <playlistName>`

**Description:** Show tracks stored in a saved playlist.

**Behavior:**

1. Validates the user.
2. Reads the playlist name.
3. Calls `IPlaylistService.ViewPlaylistAsync(guildId, playlistName)`.
4. Displays the first 10 tracks with author, title, and duration.
5. Appends an overflow line when more tracks exist.

**Error Cases:**

- Playlist does not exist -> warning.
- Playlist exists but has no tracks -> warning.
- Playlist name is invalid -> warning.
- Stored tracks cannot be deserialized for display -> error.

---

### LoadPlaylistCommand.cs

**Command:** `!loadPlaylist <playlistName>`

**Description:** Load a saved playlist into the persistent queue and start playback when the player is idle.

**Behavior:**

1. Validates the user and voice-channel state.
2. Reads the playlist name through `ICommandHelper.TryGetArgumentAsync`.
3. Calls `IPlaylistService.LoadPlaylistAsync(guildId, playlistName)`.
4. Deserializes stored track identities through `ITrackSerializer`.
5. Joins/validates the user's voice channel through `IPlayerConnectionService`.
6. Registers the playback-finished handler and enqueues the loaded tracks with `IMusicQueueService.EnqueueMany`.
7. Starts the first queued track through `ITrackPlaybackService.TryPlayNextTrackAsync` only when the player is idle.

**Error Cases:**

- Playlist does not exist -> warning.
- Playlist exists but has no tracks -> warning.
- Playlist name is invalid -> warning.
- Stored track identity cannot be deserialized -> error.
- Unexpected service failure -> error.

The command does not call `ILavaLinkService.StartPlayingQueue` directly, because that method starts the next queued track
without checking whether a track is already playing. `loadPlaylist` preserves active playback and only fills the queue
when the player is busy.

---

### RenamePlaylistCommand.cs

**Command:** `!renamePlaylist <currentName> <newName>`

**Description:** Rename an existing saved playlist.

**Behavior:**

1. Validates the user.
2. Parses current and new playlist names.
3. Calls `IPlaylistService.RenamePlaylistAsync(guildId, currentName, newName)`.
4. Sends localized responses for renamed, missing, duplicate-name, invalid-name, or unknown-error outcomes.

---

### RemoveSongFromPlaylistCommand.cs

**Command:** `!removeSong <playlistName> <trackNumber>`

**Description:** Remove one stored track from an existing saved playlist by its playlist order number.

**Behavior:**

1. Validates the user.
2. Parses playlist name and track number.
3. Rejects non-positive or non-numeric track numbers before calling the service.
4. Calls `IPlaylistService.RemoveSongFromPlaylistAsync(guildId, playlistName, trackNumber)`.
5. Removes the matching stored track and lets the repository compact playlist order numbers.

**Error Cases:**

- Playlist does not exist -> warning.
- Track number is invalid or not found -> warning.
- Playlist name is invalid -> warning.
- Unexpected service failure -> error.

## Service Dependencies

- `IPlaylistService` - playlist use-cases and result mapping.
- `IMusicQueueService` - queue persistence for loaded playlist tracks.
- `ITrackSerializer` - stored track identity deserialization.
- `IPlayerConnectionService` / `IPlaybackEventHandlerService` / `ITrackPlaybackService` - voice join and idle playback start for `loadPlaylist`.
- `ICommandHelper` - user validation and argument parsing.
- `IResponseBuilder` - success, warning, and error responses.
- `ILocalizationService` - localized command text.
- `IUserValidationService` - user/voice validation boundary.

## Safety and Limits

- Playlist command responses escape Discord mass/user/role mentions in playlist names and track metadata before sending
  text responses.
- `PlaylistOptions` controls `MaxPlaylistsPerGuild`, `MaxTracksPerPlaylist`, and `MaxImportedTracks`; defaults are used
  unless tests or startup code override the options.
- Slash commands reuse these text commands. The slash adapter quotes playlist-name arguments when needed so names with
  spaces survive the text parser.

## Persistence

Saved playlists are persisted through:

- `IPlaylistRepository` / `PlaylistRepository`
- `IPlaylistTrackRepository` / `PlaylistTrackRepository`
- `PlaylistEntity`
- `PlaylistTrackEntity`

Track identity is serialized through `ITrackSerializer`, so playlist persistence uses the same Lavalink track boundary as queue, repeat-list, and current-track storage.

## Related Components

- `Service/Music/PlaylistService/PlaylistService.cs`
- `Interface/Service/Music/PlaylistServiceInterface/`
- `Interface/Service/Persistence/IPlaylistRepository.cs`
- `Interface/Service/Persistence/IPlaylistTrackRepository.cs`
- `Persistence/Repositories/PlaylistRepository.cs`
- `Persistence/Repositories/PlaylistTrackRepository.cs`
- `localization/eng.json`
- `localization/hu.json`
