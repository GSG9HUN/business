# Slash Command Service Interfaces

This folder contains slash command adapter contracts.

## Files

### ISlashCommandExecutor.cs

Executes a `SlashCommandExecutionRequest` by resolving the matching text `ICommand` and running it through a
slash-backed `IDiscordMessage` wrapper.

### ISlashInteractionContext.cs

Framework-neutral slash interaction abstraction.

Key members:

- `GuildId`, `Guild`, `Channel`, `User`, `Member` - Discord interaction context.
- `IsDeferred`, `HasResponded` - response state tracking.
- `DeferAsync()` and `RespondAsync(...)` - interaction response operations.
- `CreateMessage(commandName, argument)` - creates an `IDiscordMessage`-compatible message for the text command pipeline.

### ISlashInteractionContextFactory.cs

Creates `ISlashInteractionContext` wrappers from DSharpPlus `CommandContext` instances.

### SlashCommandExecutionRequest.cs

Immutable request record used by slash command modules.

```csharp
public sealed record SlashCommandExecutionRequest(
    string CommandName,
    ISlashInteractionContext Context,
    string? Argument = null,
    bool RequireGuild = false,
    bool Defer = false,
    bool EnsureDeferredResponse = false);
```

## Related Components

- `Service/SlashCommands/SlashCommandExecutor.cs` - executor implementation
- `Wrapper/SlashInteractionContextFactory.cs` - factory implementation
- `Wrapper/SlashInteractionContextWrapper.cs` - DSharpPlus context wrapper
- `Wrapper/SlashInteractionMessageWrapper.cs` - message wrapper used by text commands
- `Commands/SlashCommands/` - command modules that create execution requests
