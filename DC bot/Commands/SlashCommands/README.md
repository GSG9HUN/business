# Slash Commands

This folder contains modern Discord slash command implementations.

## What's here?

Slash commands are Discord's modern interaction system that provides:
- Auto-completion
- Built-in type validation
- Rich UI with dropdowns and buttons
- Better discoverability for users
- Ephemeral (private) responses

## Available Slash Commands

- **PlaySlashCommand.cs** - `/play` - Play music with auto-complete
- **PingSlashCommand.cs** - `/ping` - Check bot latency
- **HelpSlashCommand.cs** - `/help` - Interactive help system
- **TagSlashCommand.cs** - `/tag` - Manage custom tags

## Slash Command vs Text Command

| Aspect | Text Commands | Slash Commands |
|--------|--------------|----------------|
| Prefix | `!play` | `/play` |
| Type Safety | Manual parsing | Built-in validation |
| Auto-complete | No | Yes ✅ |
| UI Integration | Basic | Rich UI ✅ |
| Discoverability | Poor | Excellent ✅ |
| Ephemeral Responses | No | Yes ✅ |

## Structure

Slash commands typically:
1. Receive an `InteractionContext` instead of `IDiscordMessage`
2. Use `SlashCommandResponseHelper` for consistent responses
3. Support options with auto-complete and validation
4. Can respond ephemerally (only visible to command issuer)

## Usage Example

```csharp
[SlashCommand("play", "Play music from URL or search query")]
public async Task Play(
    InteractionContext ctx,
    [Option("query", "URL or search query")] string query)
{
    // Defer response (slash commands must respond within 3 seconds)
    await ctx.DeferAsync();
    
    // Validate and execute
    var result = await _lavaLinkService.PlayAsync(query);
    
    // Respond with result
    await SlashCommandResponseHelper.RespondAsync(ctx, result.Message);
}
```

## Response Types

- **Immediate Response** - `ctx.CreateResponseAsync()`
- **Deferred Response** - `ctx.DeferAsync()` then `ctx.EditResponseAsync()`
- **Ephemeral Response** - Only visible to user (flags: `Ephemeral`)
- **Follow-up** - `ctx.FollowUpAsync()` for multiple messages

## Auto-Complete

Slash commands can provide auto-complete for options:

```csharp
[SlashCommand("play", "Play music")]
public async Task Play(
    InteractionContext ctx,
    [Autocomplete(typeof(MusicAutocompleteProvider))]
    [Option("query", "Search")] string query)
{
    // ...
}

public class MusicAutocompleteProvider : IAutocompleteProvider
{
    public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
    {
        var partial = ctx.OptionValue.ToString();
        var results = await SearchMusicAsync(partial);
        return results.Select(r => new DiscordAutoCompleteChoice(r.Title, r.Url));
    }
}
```

## Error Handling

```csharp
try
{
    await ctx.DeferAsync();
    // ... command logic
    await SlashCommandResponseHelper.RespondSuccessAsync(ctx, message);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Slash command failed");
    await SlashCommandResponseHelper.RespondErrorAsync(ctx, "An error occurred");
}
```

## Registration

Slash commands are automatically registered when:
1. Bot starts up
2. Commands are discovered via reflection
3. Discord API is called to register/update commands

Registration happens in `BotService` or startup configuration.

## Best Practices

- ✅ Always defer long-running operations
- ✅ Use ephemeral responses for errors
- ✅ Provide meaningful option descriptions
- ✅ Use auto-complete where applicable
- ✅ Validate options server-side (don't trust client)
- ❌ Don't forget to respond within 3 seconds
- ❌ Don't expose sensitive info in public responses
- ❌ Don't register commands on every restart (cache them)

## Migration from Text Commands

To migrate a text command to slash:
1. Convert `ICommand` to `[SlashCommand]` attribute
2. Replace `IDiscordMessage` with `InteractionContext`
3. Add `[Option]` attributes for parameters
4. Use `ctx.DeferAsync()` for long operations
5. Replace `responseBuilder.SendAsync()` with `ctx.CreateResponseAsync()`

## Testing

Slash commands are harder to test than text commands:
- Mock `InteractionContext`
- Mock `DiscordClient` responses
- Test auto-complete providers independently
- Integration tests with Discord API recommended

## Related

- **Helper/SlashCommandResponseHelper.cs** - Slash response utilities
- **Commands/** - Legacy text commands
- **Service/** - Shared business logic
- **Interface/ICommand.cs** - Text command contract

