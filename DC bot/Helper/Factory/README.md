# Helper Factory

This folder contains factory classes for creating wrapper objects.

## Overview

Factory classes centralize object creation logic for Discord wrappers.

## Files

### DiscordMessageWrapperFactory.cs

**Purpose:** Create `IDiscordMessage` wrapper instances from DSharpPlus `DiscordMessage` objects.

**Definition:**

```csharp
public static class DiscordMessageWrapperFactory
{
    public static IDiscordMessage Create(
        DiscordMessage message, 
        DiscordChannel channel, 
        DiscordUser author, 
        ILogger<DiscordMessageWrapper>? logger = null)
    {
        var discordAuthor = new DiscordUserWrapper(author);
        var discordChannel = new DiscordChannelWrapper(channel);
        
        return new DiscordMessageWrapper(
            message.Id, 
            message.Content,
            discordChannel, 
            discordAuthor, 
            message.CreationTimestamp,
            message.Embeds.ToList(), 
            message.RespondAsync,
            message.RespondAsync, 
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
    logger
);

// Now use as IDiscordMessage
await command.ExecuteAsync(wrappedMessage);
```

**Benefits:**

- **Centralizes wrapper creation** - Single place for initialization logic
- **Consistent initialization** - All wrappers created the same way
- **Simplifies testing** - Easy to mock `IDiscordMessage` in tests
- **Reduces duplication** - No repeated wrapper construction code

**Created Wrappers:**

- `DiscordMessageWrapper` - Main message wrapper
- `DiscordUserWrapper` - Wraps message author
- `DiscordChannelWrapper` - Wraps message channel

---

## Related Components

- **Wrapper/DiscordMessageWrapper.cs** - Created by this factory
- **Wrapper/DiscordUserWrapper.cs** - Created internally by factory
- **Wrapper/DiscordChannelWrapper.cs** - Created internally by factory
- **Interface/Discord/IDiscordMessage.cs** - Interface returned by factory
- **Service/CommandHandlerService.cs** - Uses factory to create wrappers

