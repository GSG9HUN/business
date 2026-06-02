# Slash Command Services

This folder contains service-layer slash command execution adapters.

## Files

### SlashCommandExecutor.cs

**Implements:** `ISlashCommandExecutor`

**Purpose:** Execute slash command requests through the existing text command pipeline.

## Behavior

- Looks up registered `ICommand` implementations by command name.
- Optionally rejects commands outside guild context with a localized slash-only response.
- Optionally defers the interaction before executing the text command.
- Creates an `IDiscordMessage`-compatible slash message through `ISlashInteractionContext.CreateMessage()`.
- Sends a localized accepted fallback when a deferred slash command finishes without producing a response.
- Handles `BotException` and unexpected exceptions without leaking framework details to the command modules.

## Related Components

- `Interface/Service/SlashCommands/README.md`
- `Wrapper/SlashInteractionContextWrapper.cs`
- `Wrapper/SlashInteractionMessageWrapper.cs`
- `Commands/SlashCommands/`
- `Commands/TextCommands/`
