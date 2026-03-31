# Wrapper

This folder contains wrapper classes that abstract the DSharpPlus Discord library.

## Overview

Wrappers implement interfaces defined in `Interface/Discord/`, providing:

- Abstraction from DSharpPlus
- Testable interfaces
- Exception handling
- Consistent Discord API interaction

## Files

### DiscordMessageWrapper.cs

**Implements:** `IDiscordMessage`

**Purpose:** Wraps DSharpPlus `DiscordMessage`.

**Properties:**

- `Id` - Message ID
- `Content` - Message text
- `Author` - Message author
- `Channel` - Message channel
- `CreatedAt` - Message creation timestamp
- `Embeds` - Embedded content

**Methods:**

- `RespondAsync()` - Send text or embed response

---

### DiscordChannelWrapper.cs

**Implements:** `IDiscordChannel`

**Purpose:** Wraps DSharpPlus `DiscordChannel`.

**Properties:**

- `Id` - Channel ID
- `Name` - Channel name
- `Guild` - Parent guild

**Methods:**

- `SendMessageAsync()` - Send message to channel
- `ToDiscordChannel()` - Get underlying DSharpPlus channel

---

### DiscordUserWrapper.cs

**Implements:** `IDiscordUser`

**Purpose:** Wraps DSharpPlus `DiscordUser`.

**Properties:**

- `Id` - User ID
- `Username` - User name
- `IsBot` - Whether user is a bot

---

### DiscordMemberWrapper.cs

**Implements:** `IDiscordMember`

**Purpose:** Wraps DSharpPlus `DiscordMember`.

**Properties:**

- `Id` - Member ID
- `Username` - Member name
- `IsBot` - Whether member is a bot
- `Guild` - Parent guild
- `VoiceState` - Voice channel state (nullable)

---

### DiscordGuildWrapper.cs

**Implements:** `IDiscordGuild`

**Purpose:** Wraps DSharpPlus `DiscordGuild`.

**Properties:**

- `Id` - Guild ID
- `Name` - Guild name

---

### DiscordVoiceStateWrapper.cs

**Implements:** `IDiscordVoiceState`

**Purpose:** Wraps DSharpPlus `DiscordVoiceState`.

**Properties:**

- `Channel` - Voice channel (nullable)
- `IsDeafened` - Whether member is deafened
- `IsMuted` - Whether member is muted

---

### LavalinkTrackWrapper.cs

**Implements:** `ILavaLinkTrack`

**Purpose:** Wraps Lavalink4NET `LavalinkTrack`.

**Properties:**

- `Title` - Track title
- `Author` - Track artist

**Methods:**

- `ToLavalinkTrack()` - Get underlying Lavalink track

---

### DiscordClientEventHandler.cs

**Purpose:** Register and manage Discord client event handlers.

**Methods:**

- `RegisterHandler()` - Register event listeners
- `OnMessageCreated()` - Handle message events
- `OnGuildMemberUpdated()` - Handle member updates
- `OnVoiceStateUpdated()` - Handle voice state changes

**Features:**

- Routes messages to command handler
- Routes reactions to reaction handler
- Tracks voice state changes

---

### DiscordClientFactory.cs

**Purpose:** Create and configure Discord client instances.

**Methods:**

- `CreateClient()` - Create configured Discord client

**Configuration:**

- Sets intents (privileged and unprivileged)
- Configures caching
- Enables message content

---

## Usage Pattern

Wrappers are created from DSharpPlus objects:

```csharp
// In CommandHandlerService
var wrappedMessage = DiscordMessageWrapperFactory.Create(
    discordMessage, 
    channel, 
    author, 
    logger
);

// Pass to command (expects IDiscordMessage interface)
await command.ExecuteAsync(wrappedMessage);
```

## Related Components

- **Interface/Discord/** - Wrapper contracts
- **Service/** - Consumes wrappers
- **Commands/** - Primary wrapper users
- **Exceptions/** - Wrapped exceptions

