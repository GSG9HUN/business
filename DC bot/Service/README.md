# Service

This folder contains the business logic layer of the application.

## What's here?

Service classes that implement core business logic:
- Music playback orchestration
- Queue management
- User validation
- Localization
- Response building
- Command handling
- Event handling

Services follow the **Service Layer** architectural pattern, sitting between commands and external dependencies (Discord, Lavalink, file system).

## Contents

### Core Services

#### LavaLinkService.cs
**Purpose:** Orchestrates music playback using Lavalink audio server.

**Key Responsibilities:**
- Connect to Lavalink server
- Play music from URLs or search queries
- Manage playback state (pause, resume, skip)
- Handle track end events
- Manage repeat modes
- Voice channel joining/leaving
- Guild-specific state management

**Key Methods:**
```csharp
Task ConnectAsync();
Task PlayAsyncUrl(IDiscordChannel channel, Uri url, IDiscordMessage message, TrackSearchMode mode);
Task PlayAsyncQuery(IDiscordChannel channel, string query, IDiscordMessage message, TrackSearchMode mode);
Task PauseAsync(IDiscordMessage message, IDiscordMember? member);
Task ResumeAsync(IDiscordMessage message, IDiscordMember? member);
Task SkipAsync(IDiscordMessage message, IDiscordMember? member);
Task LeaveVoiceChannel(IDiscordMessage message, IDiscordMember? member);
Task CleanupGuildAsync(ulong guildId);
```

**Event Management:**
- Registers `TrackEnded` event handlers per guild
- Cleans up handlers on disconnect
- Prevents duplicate handler registration
- Thread-safe event operations

See: `LavaLinkService.cs` detailed documentation

#### BotService.cs
**Purpose:** Main bot lifecycle management.

**Key Responsibilities:**
- Initialize Discord client
- Configure services
- Register command handlers
- Start/stop bot
- Graceful shutdown

**Lifecycle:**
```
Startup → ConnectAsync → Ready
         ↓
     RegisterCommands
         ↓
     StartAsync → Running
         ↓
     StopAsync → Cleanup → Stopped
```

#### CommandHandlerService.cs
**Purpose:** Route incoming Discord messages to appropriate commands.

**Key Responsibilities:**
- Parse command prefix (`!play`, `/play`)
- Match command names
- Execute command handlers
- Log command invocations
- Handle command errors

**Flow:**
```
Discord Message
  ↓
Parse Prefix → Extract Command Name
  ↓
Find ICommand Implementation
  ↓
command.ExecuteAsync(message)
  ↓
Log Result
```

### Validation Services

#### ValidationService.cs
**Purpose:** Validate players, connections, and users.

**Implements:**
- `IValidationService` - Player/connection validation
- `IUserValidationService` - User validation

**Key Methods:**
```csharp
Task<PlayerValidationResult> ValidatePlayerAsync(IAudioService audioService, ulong guildId);
Task<ConnectionValidationResult> ValidateConnectionAsync(ILavalinkPlayer connection);
Task<UserValidationResult> ValidateUserAsync(IDiscordMessage message);
bool IsBotUser(IDiscordMessage message);
```

**Validation Checks:**
- Player exists and is connected
- Connection is established
- User is not a bot
- User is in a voice channel
- User is in the same channel as bot

### Communication Services

#### ResponseBuilder.cs
**Purpose:** Build and send Discord messages consistently.

**Key Responsibilities:**
- Send plain text messages
- Send validation error messages (localized)
- Send embeds (future)
- Centralize message formatting

**Key Methods:**
```csharp
Task SendMessageAsync(IDiscordChannel channel, string message);
Task SendValidationErrorAsync(IDiscordMessage message, string errorKey);
Task SendEmbedAsync(IDiscordChannel channel, string title, string description);
```

**Benefits:**
- Consistent error formatting
- Automatic localization lookup
- Centralized retry logic
- Exception handling

#### LocalizationService.cs
**Purpose:** Multi-language support.

**Key Responsibilities:**
- Load language files (JSON)
- Retrieve localized strings by key
- Per-guild language selection
- Default language fallback

**Key Methods:**
```csharp
string Get(string key);
string Get(string key, ulong guildId);
void SetLanguage(ulong guildId, string languageCode);
string GetCurrentLanguage(ulong guildId);
```

**Language Files:**
```
localization/
├─ eng.json  (English - default)
└─ hu.json   (Hungarian)
```

**Usage:**
```csharp
var description = localizationService.Get(LocalizationKeys.PlayCommandDescription);
// Returns: "Play music from URL or search query"
```

### Utility Services

#### TrackSearchResolverService.cs
**Purpose:** Determine if input is a URL or search query.

**Key Responsibilities:**
- Parse URLs (YouTube, Spotify, SoundCloud)
- Detect search queries
- Determine appropriate `TrackSearchMode`

**Key Methods:**
```csharp
bool TryResolve(string input, out Uri? url, out TrackSearchMode searchMode);
```

**Resolution Logic:**
```
Input: "https://youtube.com/watch?v=..."
  → url = Uri, searchMode = YouTube

Input: "never gonna give you up"
  → url = null, searchMode = YouTubeMusic (search)
```

### Event Handlers

#### ReactionHandler.cs
**Purpose:** Handle Discord reaction events (emoji-based controls).

**Key Responsibilities:**
- Register reaction handlers
- Handle play/pause/skip reactions
- Manage reaction-based UI
- Unregister handlers on cleanup

**Supported Reactions:**
- ▶️ Play/Resume
- ⏸️ Pause
- ⏭️ Skip
- 🔁 Repeat
- 🔀 Shuffle

**Event Flow:**
```
Track Started Event
  ↓
Send Message with Reactions
  ↓
User Clicks Reaction
  ↓
OnReactionAdded
  ↓
Execute Action (pause/skip/etc)
```

### MusicServices/ Subfolder

Contains music-specific services:
- **MusicQueueService.cs** - Queue management
- **RepeatService.cs** - Repeat mode management
- **CurrentTrackService.cs** - Track current playing track
- **TrackNotificationService.cs** - Track notifications

See: `MusicServices/README.md` for details

## Service Lifecycle

### Registration (Program.cs)
```csharp
services.AddSingleton<ILavaLinkService, LavaLinkService>();
services.AddSingleton<IMusicQueueService, MusicQueueService>();
services.AddSingleton<IValidationService, ValidationService>();
services.AddSingleton<IUserValidationService, ValidationService>();
services.AddSingleton<ILocalizationService, LocalizationService>();
services.AddSingleton<IResponseBuilder, ResponseBuilder>();
```

### Initialization
```csharp
// Initialize per-guild state
lavalinkService.Init(guildId);
musicQueueService.Init(guildId);
```

### Usage (in Commands)
```csharp
public class PlayCommand(
    ILavaLinkService lavaLinkService,  // Injected
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder) : ICommand
{
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        // Use services
        var validationResult = await userValidation.ValidateUserAsync(message);
        await lavaLinkService.PlayAsyncQuery(...);
        await responseBuilder.SendMessageAsync(...);
    }
}
```

### Cleanup
```csharp
// Cleanup on guild leave
await lavalinkService.CleanupGuildAsync(guildId);
```

## Design Patterns

### Service Layer Pattern
Services encapsulate business logic:
```csharp
Commands → Services → External APIs
             ↓
         Business Logic
```

### Dependency Injection
Services receive dependencies via constructor:
```csharp
public class LavaLinkService(
    IAudioService audioService,           // Lavalink dependency
    IMusicQueueService musicQueueService, // Internal dependency
    ILogger<LavaLinkService> logger)      // Framework dependency
{
    // ...
}
```

### Repository Pattern (Queue)
`MusicQueueService` acts as a repository for queue data:
```csharp
void Enqueue(ulong guildId, ILavaLinkTrack track);
LavalinkTrack? Dequeue(ulong guildId);
IReadOnlyCollection<ILavaLinkTrack> ViewQueue(ulong guildId);
```

### Factory Pattern
`DiscordClientFactory` creates client instances:
```csharp
public class DiscordClientFactory
{
    public DiscordClient CreateClient(DiscordConfiguration config);
}
```

## Best Practices

- ✅ Keep services focused (Single Responsibility Principle)
- ✅ Use dependency injection
- ✅ Return result types, not exceptions for expected failures
- ✅ Log important operations
- ✅ Use async/await for I/O operations
- ✅ Thread-safe for shared state
- ✅ Clean up resources properly
- ❌ Don't put Discord-specific code in services (use wrappers)
- ❌ Don't access services from other services excessively (avoid circular dependencies)
- ❌ Don't leak implementation details in interfaces

## Testing

Services are designed to be testable:

```csharp
[Fact]
public async Task PlayAsync_ValidUrl_PlaysTrack()
{
    // Arrange
    var mockAudio = new Mock<IAudioService>();
    var mockQueue = new Mock<IMusicQueueService>();
    var service = new LavaLinkService(mockAudio.Object, mockQueue.Object, ...);

    // Act
    await service.PlayAsyncUrl(channel, new Uri("https://youtube.com/..."), message, mode);

    // Assert
    mockAudio.Verify(x => x.Tracks.LoadTracksAsync(It.IsAny<string>(), It.IsAny<TrackSearchMode>()), Times.Once);
}
```

## Error Handling

Services use structured error handling:

```csharp
try
{
    await audioService.Tracks.LoadTracksAsync(url, mode);
}
catch (HttpRequestException ex) when (ex.Message.Contains("400"))
{
    logger.LogError(ex, "Lavalink 400 Bad Request");
    await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
}
catch (Exception ex)
{
    logger.LogError(ex, "Unexpected error");
    throw new LavalinkOperationException("LoadTracks failed", ex);
}
```

## Related

- **Interface/** - Service contracts
- **Commands/** - Service consumers
- **Wrapper/** - Discord API wrappers
- **Exceptions/** - Custom exception types
- **Logging/** - Logging extensions

