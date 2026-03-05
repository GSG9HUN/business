# Interface

This folder contains all interface definitions (contracts) for the application.

## What's here?

Interface contracts that define:
- Service abstractions
- Command contracts
- Discord API wrappers
- Testability boundaries
- Dependency inversion

Interfaces enable:
- **Loose coupling** - Components depend on abstractions, not implementations
- **Testability** - Easy to mock in unit tests
- **Flexibility** - Swap implementations without changing consumers
- **SOLID principles** - Especially Dependency Inversion Principle

## Contents

### Command Contracts

#### ICommand.cs
Base contract for all text commands:
```csharp
public interface ICommand
{
    string Name { get; }           // Command name (e.g., "play")
    string Description { get; }    // User-facing description
    Task ExecuteAsync(IDiscordMessage message);
}
```

**Implementations:** `Commands/*.cs`

### Discord Wrapper Interfaces

These interfaces abstract the DSharpPlus library, making the code:
- Easier to test (mockable)
- Less coupled to DSharpPlus
- Portable to other Discord libraries

#### IDiscordMessage.cs
```csharp
public interface IDiscordMessage
{
    ulong Id { get; }
    string Content { get; }
    IDiscordUser Author { get; }
    IDiscordChannel Channel { get; }
    Task DeleteAsync();
}
```

#### IDiscordChannel.cs
```csharp
public interface IDiscordChannel
{
    ulong Id { get; }
    string Name { get; }
    IDiscordGuild Guild { get; }
    Task SendMessageAsync(string message);
}
```

#### IDiscordUser.cs
```csharp
public interface IDiscordUser
{
    ulong Id { get; }
    string Username { get; }
    bool IsBot { get; }
}
```

#### IDiscordMember.cs
```csharp
public interface IDiscordMember : IDiscordUser
{
    IDiscordGuild Guild { get; }
    IDiscordVoiceState? VoiceState { get; }
}
```

#### IDiscordGuild.cs
```csharp
public interface IDiscordGuild
{
    ulong Id { get; }
    string Name { get; }
}
```

#### IDiscordVoiceState.cs
```csharp
public interface IDiscordVoiceState
{
    IDiscordChannel? Channel { get; }
    bool IsDeafened { get; }
    bool IsMuted { get; }
}
```

**Implementations:** `Wrapper/*.cs`

### Service Interfaces

#### ILavaLinkService.cs
Music playback service:
```csharp
public interface ILavaLinkService
{
    Task PlayAsyncUrl(IDiscordChannel channel, Uri url, IDiscordMessage message, TrackSearchMode mode);
    Task PlayAsyncQuery(IDiscordChannel channel, string query, IDiscordMessage message, TrackSearchMode mode);
    Task PauseAsync(IDiscordMessage message, IDiscordMember? member);
    Task ResumeAsync(IDiscordMessage message, IDiscordMember? member);
    Task SkipAsync(IDiscordMessage message, IDiscordMember? member);
    Task LeaveVoiceChannel(IDiscordMessage message, IDiscordMember? member);
    IReadOnlyCollection<ILavaLinkTrack> ViewQueue(ulong guildId);
    void CloneQueue(ulong guildId);
    Task CleanupGuildAsync(ulong guildId);
    // ... more methods
}
```

**Implementation:** `Service/LavaLinkService.cs`

#### IMusicQueueService.cs
Queue management:
```csharp
public interface IMusicQueueService
{
    void Enqueue(ulong guildId, ILavaLinkTrack track);
    LavalinkTrack? Dequeue(ulong guildId);
    bool HasTracks(ulong guildId);
    IReadOnlyCollection<ILavaLinkTrack> ViewQueue(ulong guildId);
    void Clone(ulong guildId, LavalinkTrack currentTrack);
    IEnumerable<ILavaLinkTrack> GetRepeatableQueue(ulong guildId);
}
```

**Implementation:** `Service/MusicServices/MusicQueueService.cs`

#### IValidationService.cs
Player and connection validation:
```csharp
public interface IValidationService
{
    Task<PlayerValidationResult> ValidatePlayerAsync(IAudioService audioService, ulong guildId);
    Task<ConnectionValidationResult> ValidateConnectionAsync(ILavalinkPlayer connection);
}
```

**Implementation:** `Service/ValidationService.cs`

#### IUserValidationService.cs
User validation:
```csharp
public interface IUserValidationService
{
    Task<UserValidationResult> ValidateUserAsync(IDiscordMessage message);
    bool IsBotUser(IDiscordMessage message);
}
```

**Implementation:** `Service/ValidationService.cs`

#### ILocalizationService.cs
Multi-language support:
```csharp
public interface ILocalizationService
{
    string Get(string key);
    void SetLanguage(ulong guildId, string languageCode);
    string GetCurrentLanguage(ulong guildId);
}
```

**Implementation:** `Service/LocalizationService.cs`

#### IResponseBuilder.cs
Discord message responses:
```csharp
public interface IResponseBuilder
{
    Task SendMessageAsync(IDiscordChannel channel, string message);
    Task SendValidationErrorAsync(IDiscordMessage message, string errorKey);
    Task SendEmbedAsync(IDiscordChannel channel, string title, string description);
}
```

**Implementation:** `Service/ResponseBuilder.cs`

#### ITrackSearchResolverService.cs
Query/URL resolution:
```csharp
public interface ITrackSearchResolverService
{
    bool TryResolve(string input, out Uri? url, out TrackSearchMode searchMode);
}
```

**Implementation:** `Service/TrackSearchResolverService.cs`

#### IFileSystem.cs
File system abstraction (for testing):
```csharp
public interface IFileSystem
{
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
    bool FileExists(string path);
    string ReadAllText(string path);
    void WriteAllText(string path, string contents);
}
```

**Implementation:** `IO/PhysicalFileSystem.cs`

### Track Wrapper

#### ILavaLinkTrack.cs
Track abstraction:
```csharp
public interface ILavaLinkTrack
{
    string Title { get; }
    string Author { get; }
    LavalinkTrack ToLavalinkTrack();
}
```

**Implementation:** `Wrapper/LavalinkTrackWrapper.cs`

## Design Principles

### Dependency Inversion Principle (DIP)
High-level modules depend on abstractions:
```csharp
// ✅ Good - depends on interface
public class PlayCommand(ILavaLinkService lavaLinkService) : ICommand
{
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        await lavaLinkService.PlayAsync(...);
    }
}

// ❌ Bad - depends on concrete class
public class PlayCommand(LavaLinkService lavaLinkService) : ICommand
{
    // Tightly coupled to implementation
}
```

### Interface Segregation Principle (ISP)
Clients shouldn't depend on methods they don't use:
```csharp
// ✅ Good - focused interfaces
public interface IValidationService
{
    Task<PlayerValidationResult> ValidatePlayerAsync(...);
}

public interface IUserValidationService
{
    Task<UserValidationResult> ValidateUserAsync(...);
}

// ❌ Bad - fat interface
public interface IValidationService
{
    Task<PlayerValidationResult> ValidatePlayerAsync(...);
    Task<UserValidationResult> ValidateUserAsync(...);
    Task<ConnectionValidationResult> ValidateConnectionAsync(...);
    Task<ChannelValidationResult> ValidateChannelAsync(...);
    // ... 20 more methods
}
```

### Liskov Substitution Principle (LSP)
Any implementation should be substitutable:
```csharp
ILavaLinkService service = new LavaLinkService(...);  // Production
ILavaLinkService service = new MockLavaLinkService(); // Testing
// Both work the same way
```

## Testing with Interfaces

Interfaces enable easy mocking:

```csharp
[Fact]
public async Task PlayCommand_ValidUser_PlaysMusic()
{
    // Arrange
    var mockService = new Mock<ILavaLinkService>();
    var mockValidation = new Mock<IUserValidationService>();
    var command = new PlayCommand(mockService.Object, mockValidation.Object, ...);

    // Act
    await command.ExecuteAsync(message);

    // Assert
    mockService.Verify(x => x.PlayAsyncQuery(
        It.IsAny<IDiscordChannel>(), 
        It.IsAny<string>(), 
        It.IsAny<IDiscordMessage>(), 
        It.IsAny<TrackSearchMode>()), 
        Times.Once);
}
```

## Benefits

### 1. Testability
```csharp
// Easy to test - mock dependencies
var mockService = new Mock<ILavaLinkService>();
var command = new PlayCommand(mockService.Object);
```

### 2. Flexibility
```csharp
// Swap implementations without changing consumers
services.AddSingleton<ILavaLinkService, LavaLinkService>();      // Production
services.AddSingleton<ILavaLinkService, CachedLavaLinkService>(); // With caching
```

### 3. Decoupling
```csharp
// Commands don't know about DSharpPlus
public async Task ExecuteAsync(IDiscordMessage message) // Abstract
{
    // Not: ExecuteAsync(DiscordMessage message) // Concrete DSharpPlus type
}
```

## Best Practices

- ✅ Keep interfaces small and focused (ISP)
- ✅ Use interfaces for all services
- ✅ Wrap external libraries (DSharpPlus, Lavalink4NET)
- ✅ Name interfaces with "I" prefix (C# convention)
- ✅ Use async methods for I/O operations
- ✅ Return abstractions, not concrete types
- ❌ Don't create interfaces for everything (DTOs, POCOs)
- ❌ Don't put implementation details in interfaces
- ❌ Don't create one-to-one interface/implementation pairs unnecessarily

## When NOT to Use Interfaces

Skip interfaces for:
- **Data Transfer Objects (DTOs)** - `SerializedTrack`, `BotSettings`
- **Value Objects** - `SearchResolverOptions`
- **Configuration classes** - `LavalinkSettings`
- **Static utility classes** - `CommandValidationHelper`

## Related

- **Service/** - Interface implementations
- **Wrapper/** - Discord API wrappers
- **Commands/** - Command implementations
- **Helper/** - Result types and utilities

