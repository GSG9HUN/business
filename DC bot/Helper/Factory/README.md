# Factory

Creates the message wrapper used by command dispatch.

## Overview

- DiscordMessageWrapperFactory captures the SDK message together with resolved guild context and exposes IDiscordMessage.
- Centralizing wrapper construction keeps message routing independent of concrete wrapper constructors and easier to isolate in tests.

## Factory Classes

### DiscordMessageWrapperFactory

**File:** `DC bot/Helper/Factory/DiscordMessageWrapperFactory.cs`

**Purpose:** Creates message adapters at the DSharpPlus command boundary.

**Implements / inherits:** `IDiscordMessageFactory`.

**Methods:**

- `Create` — Constructs the adapter or runtime object from supplied context and dependencies.

```csharp
public static IDiscordMessage Create(
    DiscordMessage message,
    DiscordChannel channel,
    DiscordUser author,
    ILogger<DiscordMessageWrapper>? logger = null,
    DiscordGuild? guild = null);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var wrappedMessage = DiscordMessageWrapperFactory.Create(
    discordMessage,
    channel,
    author,
    logger,
    guild);
```

**Benefits:**

- Centralizes message wrapper construction.
- Preserves explicit guild context when the channel payload does not supply it.
- Supports the injected IDiscordMessageFactory boundary while retaining the static helper.

---

## Related Components

- `DC bot/Helper/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Wrapper/Discord/` — Wraps messages, users, guilds, channels, members, and voice state and handles Discord client events.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
