# Playlist Commands

This folder contains text commands for saved playlist management.

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
- No tracks found from the provided URL -> warning.
- Unexpected service failure -> error.

---

### DeletePlaylistCommand.cs

**Command:** `!deletePlaylist <playlistName>`

**Description:** Delete a saved playlist and its stored tracks.

**Behavior:**

1. Validates the user.
2. Reads the playlist name.
3. Calls `IPlaylistService.DeletePlaylistAsync(guildId, playlistName)`.
4. Sends a localized response for deleted, missing, or unknown-error outcomes.

---

### AddSongToPlaylistCommand.cs

**Command:** `!addSong <playlistName> <songUrl>`

**Description:** Load a single song URL and append it to an existing saved playlist.

**Behavior:**

1. Validates the user.
2. Parses playlist name and song URL.
3. Calls `IPlaylistService.AddSongToPlaylistAsync(guildId, playlistName, songUrl)`.
4. Appends the track at the next playlist order number.

**Error Cases:**

- Playlist does not exist -> warning.
- URL cannot be loaded -> warning.
- No tracks found -> warning.
- Unexpected service failure -> error.

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
- Stored tracks cannot be deserialized for display -> error.

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
- `ICommandHelper` - user validation and argument parsing.
- `IResponseBuilder` - success, warning, and error responses.
- `ILocalizationService` - localized command text.
- `IUserValidationService` - user/voice validation boundary.

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
