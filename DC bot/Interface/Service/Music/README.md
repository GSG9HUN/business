# Music Service Interfaces

This folder contains music-domain service interfaces.

## Files

### ILavaLinkService.cs

**Purpose:** Facade over music playback orchestration.

```csharp
public interface ILavaLinkService
{
    Task PauseAsync(IDiscordMessage message, IDiscordMember? member);
    Task PlayAsyncUrl(IDiscordChannel toDiscordChannel, Uri result, IDiscordMessage message, TrackSearchMode trackSearchMode);
    Task PlayAsyncQuery(IDiscordChannel toDiscordChannel, string query, IDiscordMessage message, TrackSearchMode trackSearchMode);
    Task ConnectAsync();
    Task SkipAsync(IDiscordMessage message, IDiscordMember? member);
    Task ResumeAsync(IDiscordMessage message, IDiscordMember? member);
    Task Init(ulong guildId);
    event Func<IDiscordChannel, DiscordEmbed, Task> TrackStarted;
    Task StartPlayingQueue(IDiscordMessage message, IDiscordChannel textChannel, IDiscordMember? member);
    Task LeaveVoiceChannel(IDiscordMessage message, IDiscordMember? member);
}
```

**Methods:**

- `ConnectAsync()` - Delegate Lavalink node startup
- `PlayAsyncUrl()` - Delegate URL play request handling
- `PlayAsyncQuery()` - Delegate query play request handling
- `PauseAsync()` - Delegate pause control
- `ResumeAsync()` - Delegate resume control
- `SkipAsync()` - Delegate skip control
- `StartPlayingQueue()` - Join voice and start queue
- `LeaveVoiceChannel()` - Disconnect from voice
- `Init()` - Initialize guild state

**Events:**

- `TrackStarted` - Fired when track starts playing; carries the target text channel and embed payload

**Implementation:** `Service/Music/LavaLinkService.cs`

---

### ITrackSearchResolverService.cs

**Purpose:** Resolve URLs and search queries.

**Method:**

- `ResolveSearchMode(string input)` - Determine the Lavalink `TrackSearchMode` from URL host, explicit prefix, or default search option

**Implementation:** `Service/Music/TrackSearchResolverService.cs`

---

### MusicServiceInterface/

Contains granular music service interfaces:

- `ICurrentTrackService.cs`
- `ILavalinkNodeConnectionService.cs`
- `IMusicQueueService.cs`
- `IPlaybackControlService.cs`
- `IPlaybackEventHandlerService.cs`
- `IPlayerConnectionService.cs`
- `IPlaybackRequestService.cs`
- `IRepeatService.cs`
- `ITrackEndedHandlerService.cs`
- `ITrackFormatterService.cs`
- `ITrackNotificationService.cs`
- `ITrackPlaybackService.cs`

**Implementations:** `Service/Music/MusicServices/*.cs`

---

### ProgressiveTimerInterface/

Contains `IProgressiveTimerService`, which starts/stops per-guild timer updates for the now-playing message.

**Implementation:** `Service/Music/ProgressiveTimer/ProgressiveTimerService.cs`

---

## Related Components

- **Service/Music/LavaLinkService.cs** - Main service
- **Service/Music/MusicServices/** - Granular services
- **Commands/Music/** - Use these interfaces

