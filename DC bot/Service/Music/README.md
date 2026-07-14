# Music Services

This folder contains music playback, queue management, and saved playlist services.

## Root Files

### LavaLinkService.cs

**Purpose:** Facade for Lavalink audio server and music playback operations.

**Implements:** `ILavaLinkService`

**Key Methods:**

- `ConnectAsync()` - Connect to Lavalink server
- `PlayAsyncUrl()` - Play track from URL
- `PlayAsyncQuery()` - Play track from search query
- `PauseAsync()` - Pause current track
- `ResumeAsync()` - Resume paused track
- `SkipAsync()` - Skip to next track
- `StartPlayingQueue()` - Join voice channel and start queue
- `LeaveVoiceChannel()` - Disconnect from voice
- `Init()` - Initialize guild state

**Events:**

- `TrackStarted` - Fired when track begins playing. The event publishes the target text channel and embed; `ReactionHandlerService` captures the `DiscordClient` when it registers.

**Architecture:**

- Delegates to specialized services for specific tasks
- Uses `ILavalinkNodeConnectionService` for node connection lifecycle
- Uses `IPlaybackRequestService` for URL/query play request orchestration
- Uses `IPlaybackControlService` for pause/resume/skip/leave behavior
- Uses `IPlayerConnectionService` for voice channel management
- Uses `ITrackPlaybackService` for queue track playback
- Uses `IPlaybackEventHandlerService` for event handling
- Uses `IMusicQueueService` for queue management and repeat-list queue rehydration

---

### TrackSearchResolverService.cs

**Purpose:** Resolve search queries and URLs to appropriate search modes.

**Implements:** `ITrackSearchResolverService`

**Method:**

- `ResolveSearchMode()` - Determine search mode from input string

**Supported Prefixes:**

- `spotify:` or `sptfy:` → Spotify
- `soundcloud:` or `scsearch:` → SoundCloud
- `youtubemusic:` or `ytmsearch:` → YouTube Music
- `youtube:` or `ytsearch:` → YouTube
- `applemusic:` or `amsearch:` → Apple Music
- `deezer:` or `dzsearch:` → Deezer
- `yandexmusic:` or `ymsearch:` → Yandex Music
- `bandcamp:` or `bcsearch:` → Bandcamp

**Default Search Mode:**

- Configurable via `SearchResolverOptions`
- Falls back to YouTube if not specified

For absolute URLs, the resolver also maps known hosts such as YouTube, YouTube Music, SoundCloud, Spotify, Apple Music,
Deezer, Yandex Music, and Bandcamp. Unknown absolute URLs resolve to `TrackSearchMode.None`.

---

## Subfolders

### MusicServices/

Granular music component services.

**Services:**

- `CurrentTrackService.cs`
- `LavalinkNodeConnectionService.cs`
- `MusicQueueService.cs`
- `LavalinkTrackSerializer.cs`
- `PlaybackControlService.cs`
- `PlaybackEventHandlerService.cs`
- `PlaybackRequestService.cs`
- `PlayerConnectionService.cs`
- `RepeatService.cs`
- `TrackEndedHandlerService.cs`
- `TrackFormatterService.cs`
- `TrackNotificationService.cs`
- `TrackPlaybackService.cs`

Each service implements a corresponding interface from `Interface/Service/Music/MusicServiceInterface/`.

`LavalinkTrackSerializer` is the shared track identity boundary for queue, repeat-list, and current-track persistence.

---

### PlaylistService/

Contains `PlaylistService.cs`, which implements `IPlaylistService`.

Responsibilities:

- create empty guild playlists
- save Lavalink-loaded playlist URLs into persistent playlist tracks
- append a single loaded song to an existing playlist
- remove a stored song from an existing playlist by track number
- list saved playlists with track counts
- view stored playlist tracks by deserializing saved track identifiers
- delete and rename guild playlists

---

### ProgressiveTimer/

Contains `ProgressiveTimerService.cs`, which implements `IProgressiveTimerService` and updates the now-playing message while a track is active.
The timer is started from the reaction control message flow, paused/resumed by playback controls, and stopped by skip, leave, and track-ended handling.

---

## Related Components

- **Interface/Service/Music/** - Service contracts
- **Commands/TextCommands/Music/** - Text commands that use `LavaLinkService`
- **Commands/TextCommands/Queue/** - Text commands that use `MusicQueueService`
- **Commands/TextCommands/Playlist/** - Text commands that use `PlaylistService`
- **Commands/SlashCommands/** - Slash adapters that reuse the same text command pipeline
- **Service/Music/MusicServices/** - Detailed service implementations

