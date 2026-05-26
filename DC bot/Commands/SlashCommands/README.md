# Slash Commands

This folder contains modern Discord slash command implementations.

## Overview

Slash commands are Discord's interaction system that provides:

- Auto-completion
- Built-in type validation
- Ephemeral (private) responses
- Better discoverability

Current status: these classes are source stubs only. Runtime registration is disabled in the startup composition root, and most command bodies are commented/no-op until slash command support is re-enabled and completed.

## Available Slash Commands

- `PlaySlashCommand.cs` - `/play` - source exists, playback logic is commented out
- `PingSlashCommand.cs` - `/ping` - source exists, response logic is commented out
- `HelpSlashCommand.cs` - `/help` - source exists, response logic is commented out
- `TagSlashCommand.cs` - `/tag` - source exists, member lookup/response logic is commented out

## Slash Command vs Text Command

| Aspect              | Text Commands  | Slash Commands      |
|---------------------|----------------|---------------------|
| Prefix              | `!play`        | `/play`             |
| Type Safety         | Manual parsing | Built-in validation |
| Auto-complete       | No             | Yes                 |
| Discoverability     | Poor           | Excellent           |
| Ephemeral Responses | No             | Yes                 |

## Structure

Slash commands:

1. Receive an `InteractionContext` instead of `IDiscordMessage`
2. Use `SlashCommandResponseHelper` for responses
3. Support options with validation
4. Can respond ephemerally (only visible to user)

## Implementation Pattern

```csharp
[SlashCommand("play", "Play music from URL or search query")]
public async Task Play(
    InteractionContext ctx,
    [Option("query", "URL or search query")] string query)
{
    await ctx.DeferAsync(); // Must respond within 3 seconds
    
    // Execute logic
    await lavaLinkService.PlayAsync(query);
    
    await SlashCommandResponseHelper.RespondAsync(ctx, "Playing...");
}
```

## Response Types

- **Deferred Response** - `ctx.DeferAsync()` then `ctx.EditResponseAsync()`
- **Ephemeral Response** - Only visible to user
- **Follow-up** - `ctx.FollowUpAsync()` for multiple messages

## Registration

Slash command registration is currently disabled. `Startup/BotServiceProviderFactory.cs` does not register slash command services, so these commands are not active at runtime.

## Related Components

- `Helper/SlashCommandResponseHelper.cs` - Slash response utilities
- `Commands/` - Text command equivalents
- `Service/` - Shared business logic

