# Slash Command Services

This folder contains service-layer slash command execution adapters.

## Why This Folder Exists

Slash command modules should stay thin. They receive DSharpPlus interaction input, but the bot already has a mature text-command pipeline for validation, localization, and command behavior.

This service layer bridges slash commands into that existing pipeline so slash commands do not duplicate the behavior of text commands.

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

## Design Rule

New slash command modules should delegate through this service when they mirror an existing text command. Only add slash-specific behavior in the module when Discord interaction UX requires it, such as option choices or member selectors.

## Related Components

- `Interface/Service/SlashCommands/README.md`
- `Wrapper/SlashInteractionContextWrapper.cs`
- `Wrapper/SlashInteractionMessageWrapper.cs`
- `Commands/SlashCommands/`
- `Commands/TextCommands/`
