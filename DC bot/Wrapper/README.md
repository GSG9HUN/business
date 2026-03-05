# Wrapper

This folder contains wrapper classes that abstract the DSharpPlus Discord library.

## What's here?

Wrapper implementations that:
- Decouple application code from DSharpPlus
- Provide testable interfaces
- Simplify Discord API interactions
- Enable library migration if needed
- Centralize Discord-specific logic

These wrappers implement the interfaces defined in `Interface/IDiscord*.cs`.

## Why Wrap DSharpPlus?

### Without Wrappers (❌ Bad)
```csharp
// Command directly uses DSharpPlus types
public class PlayCommand : ICommand
{
    public async Task ExecuteAsync(DiscordMessage message) // DSharpPlus type!
    {
        var channel = message.Channel;
        await channel.SendMessageAsync("Playing...");
    }
}

// Problems:
// 1. Can't mock DiscordMessage for testing
// 2. Tightly coupled to DSharpPlus
// 3. Hard to switch Discord libraries
// 4. DSharpPlus exceptions leak everywhere
```

### With Wrappers (✅ Good)
```csharp
// Command uses interface
public class PlayCommand : ICommand
{
    public async Task ExecuteAsync(IDiscordMessage message) // Interface!
    {
        var channel = message.Channel;
        await channel.SendMessageAsync("Playing...");
    }
}

// Benefits:
// 1. Easy to mock in tests
// 2. Loosely coupled
// 3. Can swap libraries (DSharpPlus → Discord.Net)
// 4. Wrapper handles exceptions
```

## Contents

### DiscordMessageWrapper.cs
Wraps `DiscordMessage` from DSharpPlus.

**Interface:** `IDiscordMessage`

**Key Properties:**
```csharp
public class DiscordMessageWrapper : IDiscordMessage
{
    public ulong Id { get; }
    public string Content { get; }
    public IDiscordUser Author { get; }
    public IDiscordChannel Channel { get; }
    
    public async Task DeleteAsync();
}
```

**Usage:**
```csharp
// Create wrapper
var wrappedMessage = new DiscordMessageWrapper(discordMessage, logger);

// Use in command
await command.ExecuteAsync(wrappedMessage);
```

### DiscordChannelWrapper.cs
Wraps `DiscordChannel` from DSharpPlus.

**Interface:** `IDiscordChannel`

**Key Properties:**
```csharp
public class DiscordChannelWrapper : IDiscordChannel
{
    public ulong Id { get; }
    public string Name { get; }
    public IDiscordGuild Guild { get; }
    
    public async Task SendMessageAsync(string message);
}
```

**Error Handling:**
```csharp
public async Task SendMessageAsync(string message)
{
    try
    {
        await _discordChannel.SendMessageAsync(message);
    }
    catch (DiscordException ex)
    {
        _logger.LogError(ex, "Failed to send message to channel {ChannelId}", Id);
        throw new MessageSendException("Failed to send message", ex);
    }
}
```

### DiscordUserWrapper.cs
Wraps `DiscordUser` from DSharpPlus.

**Interface:** `IDiscordUser`

**Key Properties:**
```csharp
public class DiscordUserWrapper : IDiscordUser
{
    public ulong Id { get; }
    public string Username { get; }
    public bool IsBot { get; }
}
```

### DiscordMemberWrapper.cs
Wraps `DiscordMember` from DSharpPlus.

**Interface:** `IDiscordMember`

**Key Properties:**
```csharp
public class DiscordMemberWrapper : IDiscordMember
{
    public ulong Id { get; }
    public string Username { get; }
    public bool IsBot { get; }
    public IDiscordGuild Guild { get; }
    public IDiscordVoiceState? VoiceState { get; }
}
```

**Voice State:**
```csharp
var member = new DiscordMemberWrapper(discordMember);
var voiceChannel = member.VoiceState?.Channel;

if (voiceChannel != null)
{
    // User is in a voice channel
}
```

### DiscordGuildWrapper.cs
Wraps `DiscordGuild` from DSharpPlus.

**Interface:** `IDiscordGuild`

**Key Properties:**
```csharp
public class DiscordGuildWrapper : IDiscordGuild
{
    public ulong Id { get; }
    public string Name { get; }
}
```

### DiscordVoiceStateWrapper.cs
Wraps `DiscordVoiceState` from DSharpPlus.

**Interface:** `IDiscordVoiceState`

**Key Properties:**
```csharp
public class DiscordVoiceStateWrapper : IDiscordVoiceState
{
    public IDiscordChannel? Channel { get; }
    public bool IsDeafened { get; }
    public bool IsMuted { get; }
}
```

### LavalinkTrackWrapper.cs
Wraps `LavalinkTrack` from Lavalink4NET.

**Interface:** `ILavaLinkTrack`

**Key Properties:**
```csharp
public class LavalinkTrackWrapper : ILavaLinkTrack
{
    public string Title { get; }
    public string Author { get; }
    
    public LavalinkTrack ToLavalinkTrack();
}
```

**Purpose:**
- Allow serialization/deserialization
- Provide consistent interface for queue storage
- Enable mocking in tests

### DiscordClientEventHandler.cs
Handles Discord client events.

**Purpose:**
- Register event handlers
- Route events to appropriate handlers
- Unregister handlers on cleanup

**Events Handled:**
- `MessageCreated` → Command handler
- `GuildMemberUpdated` → State updates
- `VoiceStateUpdated` → Track voice changes

### DiscordClientFactory.cs
Factory for creating Discord client instances.

**Purpose:**
- Centralize client configuration
- Configure intents and caching
- Set up event handlers
- Support dependency injection

**Usage:**
```csharp
public class DiscordClientFactory
{
    public DiscordClient CreateClient(DiscordConfiguration config)
    {
        var client = new DiscordClient(config);
        
        // Configure intents
        config.Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContent;
        
        // Configure caching
        config.MessageCacheSize = 1024;
        
        return client;
    }
}
```

## Factory Pattern

Wrappers are typically created via factory:

```csharp
public class DiscordMessageWrapperFactory
{
    public IDiscordMessage CreateMessage(DiscordMessage message)
    {
        var channel = new DiscordChannelWrapper(message.Channel, _channelLogger);
        var author = new DiscordUserWrapper(message.Author);
        return new DiscordMessageWrapper(message, channel, author, _messageLogger);
    }
}
```

**Benefits:**
- Consistent wrapper creation
- Proper dependency injection
- Nested wrapper creation
- Centralized logic

## Testing with Wrappers

Wrappers make testing trivial:

```csharp
[Fact]
public async Task PlayCommand_SendsMessage()
{
    // Arrange
    var mockMessage = new Mock<IDiscordMessage>();
    var mockChannel = new Mock<IDiscordChannel>();
    mockMessage.Setup(m => m.Channel).Returns(mockChannel.Object);
    
    var command = new PlayCommand(...);

    // Act
    await command.ExecuteAsync(mockMessage.Object);

    // Assert
    mockChannel.Verify(c => c.SendMessageAsync(It.IsAny<string>()), Times.Once);
}
```

Without wrappers, you'd need to:
1. Create real Discord objects (impossible)
2. Use reflection hacks
3. Integration tests only (slow)

## Exception Mapping

Wrappers translate Discord exceptions to custom exceptions:

```csharp
public async Task SendMessageAsync(string message)
{
    try
    {
        await _channel.SendMessageAsync(message);
    }
    catch (NotFoundException ex)
    {
        throw new MessageSendException("Channel not found", ex);
    }
    catch (UnauthorizedException ex)
    {
        throw new MessageSendException("Missing permissions", ex);
    }
    catch (RateLimitException ex)
    {
        _logger.LogWarning("Rate limited, retrying...");
        await Task.Delay(ex.RetryAfter);
        await _channel.SendMessageAsync(message); // Retry
    }
}
```

## Logging

Wrappers centralize logging:

```csharp
public class DiscordChannelWrapper
{
    private readonly ILogger<DiscordChannelWrapper> _logger;

    public async Task SendMessageAsync(string message)
    {
        _logger.LogDebug("Sending message to channel {ChannelId}: {Message}", 
            Id, message.Substring(0, Math.Min(50, message.Length)));
        
        await _channel.SendMessageAsync(message);
        
        _logger.LogInformation("Message sent to channel {ChannelId}", Id);
    }
}
```

## Null Handling

Wrappers handle Discord's nullable types:

```csharp
public class DiscordMemberWrapper : IDiscordMember
{
    public IDiscordVoiceState? VoiceState 
    {
        get
        {
            var voiceState = _member.VoiceState;
            if (voiceState == null) return null;
            
            return new DiscordVoiceStateWrapper(voiceState);
        }
    }
}
```

## Performance Considerations

Wrappers add minimal overhead:
- **Memory:** One wrapper object per Discord object (~50 bytes)
- **CPU:** Negligible (simple property access)
- **I/O:** None (wrappers don't do I/O)

Trade-off:
- ❌ Slight memory overhead
- ✅ Massive testability improvement
- ✅ Cleaner architecture
- ✅ Future-proof (library migration)

## Migration Example

If switching from DSharpPlus to Discord.Net:

**Before:** 50+ files need changes  
**After:** Only wrapper implementations need changes

```csharp
// DSharpPlus implementation
public class DiscordChannelWrapper : IDiscordChannel
{
    private readonly DiscordChannel _channel; // DSharpPlus type
    // ...
}

// Discord.Net implementation (only this file changes!)
public class DiscordChannelWrapper : IDiscordChannel
{
    private readonly IMessageChannel _channel; // Discord.Net type
    // ...
}
```

## Best Practices

- ✅ Always use interfaces (`IDiscordMessage`) in application code
- ✅ Create wrappers via factory
- ✅ Log important operations
- ✅ Handle Discord exceptions in wrappers
- ✅ Map exceptions to custom types
- ✅ Keep wrappers thin (no business logic)
- ❌ Don't expose DSharpPlus types in wrapper interfaces
- ❌ Don't put business logic in wrappers
- ❌ Don't bypass wrappers (use interfaces everywhere)

## Related

- **Interface/IDiscord*.cs** - Wrapper contracts
- **Exceptions/** - Custom exception types
- **Service/** - Wrapper consumers
- **Commands/** - Primary wrapper users

