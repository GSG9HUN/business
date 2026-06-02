# Slash Commands

This folder contains Discord slash command modules for the bot.

## Current Status

Slash commands are active and registered from `Startup/BotServiceProviderFactory.cs` through `DSharpPlus.Commands` with
`SlashCommandProcessor`.

The modules are intentionally thin adapters:

1. DSharpPlus receives the slash interaction through `SlashCommandContext`.
2. The module wraps the interaction as `ISlashInteractionContext`.
3. `ISlashCommandExecutor` creates an `IDiscordMessage`-compatible wrapper.
4. The existing text command implementation executes the real command logic.

This keeps text and slash command behavior aligned and avoids duplicating music, validation, localization, queue, and
response logic.

## Folder Structure

- `Music/` - playback slash commands.
- `Queue/` - queue management slash commands.
- `Utility/` - general bot slash commands.

Each subfolder has its own README with command-specific behavior.

## Available Slash Commands

### Music

- `/play query:<url-or-search>` -> `PlayCommand`
- `/join` -> `JoinCommand`
- `/pause` -> `PauseCommand`
- `/resume` -> `ResumeCommand`
- `/skip` -> `SkipCommand`
- `/leave` -> `LeaveCommand`

### Queue

- `/queue` -> `ViewQueueCommand`
- `/shuffle` -> `ShuffleCommand`
- `/repeat track` -> `RepeatCommand`
- `/repeat list` -> `RepeatListCommand`
- `/clear confirm:<true>` -> `ClearCommand`

### Utility

- `/ping` -> `PingCommand`
- `/help` -> `HelpCommand`
- `/tag user:<member>` -> `TagCommand`
- `/language language:<eng|hu>` -> `LanguageCommand`

## Related Components

- `Interface/Service/SlashCommands/ISlashInteractionContext.cs`
- `Interface/Service/SlashCommands/ISlashInteractionContextFactory.cs`
- `Interface/Service/SlashCommands/ISlashCommandExecutor.cs`
- `Service/SlashCommands/SlashCommandExecutor.cs`
- `Wrapper/SlashInteractionContextFactory.cs`
- `Wrapper/SlashInteractionContextWrapper.cs`
- `Wrapper/SlashInteractionMessageWrapper.cs`

## Package Notes

The project uses DSharpPlus `5.0.0-nightly-02574` consistently for the DSharpPlus packages used here. Because the
DSharpPlus nightly package removes old sharded client types used by the stable Lavalink integration package, the
DSharpPlus Lavalink adapter is `Lavalink4NET.DSharpPlus.Nightly`.

The legacy `DSharpPlus.SlashCommands` package is intentionally not used.

## Tests

Slash command coverage includes:

- Unit tests for executor behavior and module delegation.
- Integration tests for startup registration and DI resolution.
- E2E-category pipeline tests for active slash command adapters.

Live Discord slash invocation requires a user/client-side command trigger and is tracked by the manual smoke checklist.
