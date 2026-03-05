# Helper

This folder contains utility classes and helper methods that provide common functionality across the application.

## What's here?

Reusable helper classes and data structures that:
- Reduce code duplication
- Provide common validation logic
- Encapsulate result types
- Offer factory methods
- Support command and service operations

## Contents

### CommandValidationHelper.cs
Static utility class for common command validation patterns.

**Purpose:** Centralize repetitive validation logic used across multiple commands.

**Key Methods:**
```csharp
public static class CommandValidationHelper
{
    // Validate user (not bot, in voice channel)
    public static async Task<UserValidationResult?> TryValidateUserAsync(
        IUserValidationService userValidation,
        IResponseBuilder responseBuilder,
        IDiscordMessage message);

    // Extract command arguments from message
    public static async Task<string?> TryGetArgumentAsync(
        IDiscordMessage message,
        IResponseBuilder responseBuilder,
        ILogger logger,
        string commandName);
}
```

**Usage:**
```csharp
// In PlayCommand.cs
public async Task ExecuteAsync(IDiscordMessage message)
{
    // Validate user
    var validationResult = await CommandValidationHelper
        .TryValidateUserAsync(userValidation, responseBuilder, message);
    if (validationResult is null) return; // Validation failed, error sent to user

    // Get command arguments
    var query = await CommandValidationHelper
        .TryGetArgumentAsync(message, responseBuilder, logger, Name);
    if (query is null) return; // No arguments provided, error sent to user

    // Proceed with command logic
    await lavaLinkService.PlayAsync(query);
}
```

### Result Types

#### UserValidationResult.cs
Encapsulates user validation outcome:
```csharp
public class UserValidationResult
{
    public bool IsValid { get; init; }
    public string ErrorKey { get; init; }
    public IDiscordMember? Member { get; init; }
}
```

**Usage:**
```csharp
var result = await validationService.ValidateUserAsync(message);
if (!result.IsValid)
{
    await responseBuilder.SendValidationErrorAsync(message, result.ErrorKey);
    return;
}

var voiceChannel = result.Member?.VoiceState?.Channel;
```

#### PlayerValidationResult.cs
Encapsulates player validation outcome:
```csharp
public class PlayerValidationResult
{
    public bool IsValid { get; init; }
    public string ErrorKey { get; init; }
    public ILavalinkPlayer? Player { get; init; }
}
```

**Usage:**
```csharp
var result = await validationService.ValidatePlayerAsync(audioService, guildId);
if (!result.IsValid)
{
    logger.LogWarning("Player validation failed: {ErrorKey}", result.ErrorKey);
    return;
}

var player = result.Player;
```

#### ConnectionValidationResult.cs
Encapsulates connection validation outcome:
```csharp
public class ConnectionValidationResult
{
    public bool IsValid { get; init; }
    public string ErrorKey { get; init; }
    public ILavalinkPlayer? Connection { get; init; }
}
```

**Usage:**
```csharp
var result = await validationService.ValidateConnectionAsync(connection);
if (!result.IsValid)
{
    await responseBuilder.SendValidationErrorAsync(message, result.ErrorKey);
    return;
}
```

### SlashCommandResponseHelper.cs
Utility for consistent slash command responses.

**Purpose:** Standardize interaction responses for slash commands.

**Key Methods:**
```csharp
public static class SlashCommandResponseHelper
{
    // Send success response
    public static async Task RespondSuccessAsync(
        InteractionContext ctx, 
        string message);

    // Send error response (ephemeral)
    public static async Task RespondErrorAsync(
        InteractionContext ctx, 
        string error);

    // Defer response for long operations
    public static async Task DeferAsync(
        InteractionContext ctx, 
        bool ephemeral = false);
}
```

**Usage:**
```csharp
[SlashCommand("play", "Play music")]
public async Task Play(InteractionContext ctx, string query)
{
    await SlashCommandResponseHelper.DeferAsync(ctx);
    
    try
    {
        await lavaLinkService.PlayAsync(query);
        await SlashCommandResponseHelper.RespondSuccessAsync(ctx, "Playing music!");
    }
    catch (Exception ex)
    {
        await SlashCommandResponseHelper.RespondErrorAsync(ctx, "Failed to play");
    }
}
```

### DiscordMessageWrapperFactory.cs
Factory for creating Discord wrapper objects.

**Purpose:** Centralize creation of wrapper instances with proper dependency injection.

**Key Methods:**
```csharp
public class DiscordMessageWrapperFactory
{
    public IDiscordMessage CreateMessage(DiscordMessage message);
    public IDiscordChannel CreateChannel(DiscordChannel channel);
    public IDiscordMember CreateMember(DiscordMember member);
}
```

**Usage:**
```csharp
var wrappedMessage = messageFactory.CreateMessage(discordMessage);
await command.ExecuteAsync(wrappedMessage);
```

### SearchResolverOptions.cs
Configuration options for search/URL resolution.

**Purpose:** Configure how the bot resolves music queries (URL vs search).

```csharp
public class SearchResolverOptions
{
    public bool PreferYouTube { get; init; } = true;
    public bool AllowSpotify { get; init; } = true;
    public int MaxSearchResults { get; init; } = 5;
}
```

**Usage:**
```csharp
services.Configure<SearchResolverOptions>(options =>
{
    options.PreferYouTube = true;
    options.MaxSearchResults = 10;
});
```

### SerializedTrack.cs
Data transfer object for persisting track information.

**Purpose:** Serialize/deserialize Lavalink tracks for queue persistence.

```csharp
public class SerializedTrack
{
    public string Title { get; init; }
    public string Author { get; init; }
    public string Uri { get; init; }
    public long DurationMs { get; init; }
}
```

**Usage:**
```csharp
// Save queue to file
var serialized = queue.Select(t => new SerializedTrack
{
    Title = t.Title,
    Author = t.Author,
    Uri = t.Uri.ToString(),
    DurationMs = t.Duration.TotalMilliseconds
});

File.WriteAllText(path, JsonSerializer.Serialize(serialized));
```

## Design Patterns

### Result Pattern
Instead of throwing exceptions for expected failures, return result objects:
```csharp
// ❌ Bad - exceptions for control flow
try
{
    var user = GetUser();
}
catch (UserNotFoundException)
{
    // Expected case
}

// ✅ Good - result object
var result = TryGetUser();
if (!result.IsValid)
{
    // Expected case, no exception
}
```

### Static Helper Pattern
For stateless utility methods:
```csharp
public static class CommandValidationHelper
{
    // No instance state, pure functions
    public static async Task<UserValidationResult?> TryValidateUserAsync(...) { }
}
```

### Factory Pattern
For complex object creation:
```csharp
public class DiscordMessageWrapperFactory
{
    public IDiscordMessage CreateMessage(DiscordMessage msg)
    {
        return new DiscordMessageWrapper(msg, _logger, _serviceProvider);
    }
}
```

## Best Practices

- ✅ Use result types instead of exceptions for expected failures
- ✅ Keep helpers stateless (static or with minimal dependencies)
- ✅ Provide clear, descriptive method names
- ✅ Use async/await for I/O operations
- ✅ Return early on validation failures
- ✅ Log at appropriate levels
- ❌ Don't put business logic in helpers (use services)
- ❌ Don't create "god" helper classes (keep focused)
- ❌ Don't hide important errors (propagate when needed)

## Testing

Helper classes should be easy to unit test:

```csharp
[Fact]
public async Task TryValidateUserAsync_ValidUser_ReturnsResult()
{
    // Arrange
    var mockValidation = new Mock<IUserValidationService>();
    mockValidation.Setup(x => x.ValidateUserAsync(It.IsAny<IDiscordMessage>()))
        .ReturnsAsync(new UserValidationResult { IsValid = true });

    // Act
    var result = await CommandValidationHelper.TryValidateUserAsync(
        mockValidation.Object, 
        responseBuilder, 
        message);

    // Assert
    Assert.NotNull(result);
    Assert.True(result.IsValid);
}
```

## Related

- **Commands/** - Primary consumers of helpers
- **Service/** - Business logic layer
- **Interface/** - Contracts for validation and services
- **Wrapper/** - Discord API wrappers

