# Slash Commands

This folder contains modern Discord slash command implementations.

## Overview

Slash commands are Discord's interaction system that provides:
- Auto-completion
- Built-in type validation
- Ephemeral (private) responses
- Better discoverability

## Available Slash Commands

- `PlaySlashCommand.cs` - `/play` - Play music
- `PingSlashCommand.cs` - `/ping` - Check bot latency
- `HelpSlashCommand.cs` - `/help` - Interactive help
- `TagSlashCommand.cs` - `/tag` - Manage custom tags

## Slash Command vs Text Command

| Aspect | Text Commands | Slash Commands |
|--------|--------------|----------------|
| Prefix | `!play` | `/play` |
| Type Safety | Manual parsing | Built-in validation |
| Auto-complete | No | Yes |
| Discoverability | Poor | Excellent |
| Ephemeral Responses | No | Yes |

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

Slash commands are registered when the bot starts up via DSharpPlus discovery.

## Related Components

- `Helper/SlashCommandResponseHelper.cs` - Slash response utilities
- `Commands/` - Text command equivalents
- `Service/` - Shared business logic

