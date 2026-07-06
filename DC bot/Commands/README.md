# Commands

This folder contains the Discord command presentation layer.

## Overview

Commands handle user input from Discord and delegate business logic to services. The bot currently exposes two command
surfaces:

1. **TextCommands** - prefix-based message commands such as `!play`, `!pause`, and `!viewList`.
2. **SlashCommands** - Discord application commands such as `/play`, `/pause`, and `/queue`.

Slash commands are intentionally thin adapters. They create a slash execution request and reuse the existing text command
pipeline through `ISlashCommandExecutor`, so validation, localization, playback, persistence, and response behavior stay
aligned between both command surfaces.

## Folder Structure

### TextCommands/

Text command implementations grouped by domain.

- `Music/` - `PlayCommand`, `PauseCommand`, `ResumeCommand`, `SkipCommand`, `JoinCommand`, `LeaveCommand`
- `Queue/` - `ViewQueueCommand`, `ShuffleCommand`, `RepeatCommand`, `RepeatListCommand`, `ClearCommand`
- `Utility/` - `HelpCommand`, `PingCommand`, `LanguageCommand`, `TagCommand`

### SlashCommands/

Slash command modules grouped to mirror the text command domains.

- `Music/` - `/join`, `/play`, `/pause`, `/resume`, `/skip`, `/leave`
- `Queue/` - `/queue`, `/shuffle`, `/repeat track`, `/repeat list`, `/clear`
- `Utility/` - `/help`, `/ping`, `/language`, `/tag`

Runtime registration is composed by `Startup/BotServiceProviderFactory.cs` and grouped in `Startup/DependencyInjection/CommandServiceCollectionExtensions.cs` through `AddCommandServices()`, `DSharpPlus.Commands`, and `SlashCommandProcessor`.

## Text Command Contract

Text commands implement `ICommand`:

```csharp
public interface ICommand
{
    string Name { get; }
    string Description { get; }
    Task ExecuteAsync(IDiscordMessage message);
}
```

## Text Command Flow

```text
Discord Message
    -> CommandHandlerService
    -> Parse prefix and command name
    -> Resolve ICommand through ICommandRegistry
    -> command.ExecuteAsync(message)
    -> Validate input and user state
    -> Call domain services
    -> Send localized response
```

## Slash Command Flow

```text
Discord Interaction
    -> DSharpPlus Commands / SlashCommandProcessor
    -> Slash command module
    -> ISlashInteractionContextFactory
    -> ISlashCommandExecutor
    -> Existing ICommand implementation
```

## Common Text Command Pattern

```csharp
public async Task ExecuteAsync(IDiscordMessage message)
{
    logger.CommandInvoked(Name);

    var validationResult = await commandHelper.TryValidateUserAsync(
        userValidation, responseBuilder, message);
    if (validationResult is null) return;

    await service.DoSomethingAsync();

    logger.CommandExecuted(Name);
}
```

## Common Slash Command Pattern

```csharp
public Task ExecuteAsync(ISlashInteractionContext context)
{
    return slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(
        "commandName",
        context,
        RequireGuild: true,
        Defer: true));
}
```

## Dependencies

Commands typically inject a subset of:

- `ILavaLinkService` / `IMusicQueueService` - music and queue orchestration
- `IUserValidationService` - user and voice-state validation
- `IResponseBuilder` - Discord response sending
- `ILocalizationService` - localized response text
- `ICommandHelper` - text command argument and validation helpers
- `ICommandRegistry` - registered text command enumeration/lookup used by help and routing
- `ISlashCommandExecutor` - slash-to-text command adapter execution
- `ISlashInteractionContextFactory` - slash context wrapper creation
- `ILogger<T>` - structured logging

## Registration

Commands are registered in `Startup/DependencyInjection/CommandServiceCollectionExtensions.cs`:

```csharp
services.AddCommandServices();
```

`AddCommandServices()` also registers the slash modules with the DSharpPlus Commands extension and `SlashCommandProcessor`.

## Related Components

- `Interface/ICommand.cs` - text command contract
- `Interface/Service/SlashCommands/` - slash adapter contracts
- `Service/Core/CommandHandlerService.cs` - text command routing
- `Service/Core/CommandValidationService.cs` - command argument helpers
- `Service/Core/ValidationService.cs` - validation services
- `Service/SlashCommands/SlashCommandExecutor.cs` - slash-to-text command execution
- `Wrapper/SlashInteractionContextWrapper.cs` - slash context wrapper
