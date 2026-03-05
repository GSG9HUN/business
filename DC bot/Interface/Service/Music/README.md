# Music Service Interfaces

This folder contains music-domain service interfaces.

## Files

### ILavaLinkService.cs

**Purpose:** Music playback orchestration.

```csharp
public interface ILavaLinkService
{
    Task PauseAsync(IDiscordMessage message, IDiscordMember? member);
    Task PlayAsyncUrl(IDiscordChannel toDiscordChannel, Uri result, IDiscordMessage message, TrackSearchMode trackSearchMode);
    Task PlayAsyncQuery(IDiscordChannel toDiscordChannel, string query, IDiscordMessage message, TrackSearchMode trackSearchMode);
    Task ConnectAsync();
    Task SkipAsync(IDiscordMessage message, IDiscordMember? member);
    Task ResumeAsync(IDiscordMessage message, IDiscordMember? member);
    void Init(ulong guildId);
    event Func<IDiscordChannel, DiscordClient, string, Task> TrackStarted;
    Task StartPlayingQueue(IDiscordMessage message, IDiscordChannel textChannel, IDiscordMember? member);
    Task LeaveVoiceChannel(IDiscordMessage message, IDiscordMember? member);
}
```

**Methods:**
- `ConnectAsync()` - Connect to Lavalink server
- `PlayAsyncUrl()` - Play track from URL
- `PlayAsyncQuery()` - Play track from search query
- `PauseAsync()` - Pause current track
- `ResumeAsync()` - Resume paused track
- `SkipAsync()` - Skip to next track
- `StartPlayingQueue()` - Join voice and start queue
- `LeaveVoiceChannel()` - Disconnect from voice
- `Init()` - Initialize guild state

**Events:**
- `TrackStarted` - Fired when track starts playing

**Implementation:** `Service/Music/LavaLinkService.cs`

---

### ITrackSearchResolverService.cs

**Purpose:** Resolve URLs and search queries.

**Implementation:** `Service/Music/TrackSearchResolverService.cs`

---

### MusicServiceInterface/

Contains granular music service interfaces:
- `ICurrentTrackService.cs`
- `IMusicQueueService.cs`
- `IPlaybackEventHandlerService.cs`
- `IPlayerConnectionService.cs`
- `IRepeatService.cs`
- `ITrackEndedHandlerService.cs`
- `ITrackFormatterService.cs`
- `ITrackNotificationService.cs`
- `ITrackPlaybackService.cs`

**Implementations:** `Service/Music/MusicServices/*.cs`

---

## Related Components

- **Service/Music/LavaLinkService.cs** - Main service
- **Service/Music/MusicServices/** - Granular services
- **Commands/Music/** - Use these interfaces

