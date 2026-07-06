# Helper Factory

This folder contains factory classes for creating wrapper objects.

## Overview

Factory classes centralize object creation logic for Discord wrappers.

## Files

### DiscordMessageWrapperFactory.cs

**Purpose:** Create `IDiscordMessage` wrapper instances from DSharpPlus `DiscordMessage` objects.

**Definition:**

```csharp
public class DiscordMessageWrapperFactory : IDiscordMessageFactory
{
    public static IDiscordMessage Create(
        DiscordMessage message,
        DiscordChannel channel,
        DiscordUser author,
        ILogger<DiscordMessageWrapper>? logger = null,
        DiscordGuild? guild = null)
    {
        var discordAuthor = new DiscordUserWrapper(author);
        var discordChannel = new DiscordChannelWrapper(channel, guild: guild);

        return new DiscordMessageWrapper(
            message.Id,
            message.Content,
            discordChannel,
            discordAuthor,
            message.CreationTimestamp,
            message.Embeds?.ToList() ?? [],
            message.RespondAsync,
            message.RespondAsync,
            builder => message.ModifyAsync(builder),
            logger
        );
    }
}
```

**Usage:**

```csharp
// In event handlers or service code
var wrappedMessage = DiscordMessageWrapperFactory.Create(
    discordMessage,
    channel,
    author,
    logger,
    guild
);

// Now use as IDiscordMessage
await command.ExecuteAsync(wrappedMessage);
```

**Benefits:**

- **Centralizes wrapper creation** - Single place for initialization logic
- **Consistent initialization** - All wrappers created the same way
- **Simplifies testing** - `CommandHandlerService` can receive an `IDiscordMessageFactory` test double
- **Reduces duplication** - No repeated wrapper construction code

**Created Wrappers:**

- `DiscordMessageWrapper` - Main message wrapper
- `DiscordUserWrapper` - Wraps message author
- `DiscordChannelWrapper` - Wraps message channel and optional explicit guild context

---

## Related Components

- **Wrapper/DiscordMessageWrapper.cs** - Created by this factory
- **Wrapper/DiscordUserWrapper.cs** - Created internally by factory
- **Wrapper/DiscordChannelWrapper.cs** - Created internally by factory
- **Interface/Discord/IDiscordMessage.cs** - Interface returned by factory
- **Interface/Discord/IDiscordMessageFactory.cs** - Injectable factory boundary
- **Service/Core/CommandHandlerService.cs** - Uses the injectable factory to create wrappers
- **Service/ReactionHandler/ReactionContextFactory.cs** - Uses explicit guild context when building reaction event wrappers

