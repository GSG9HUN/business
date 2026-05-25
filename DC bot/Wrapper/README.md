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
- `ModifyAsync()` - Modify the wrapped Discord message

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
- `Mention` - Discord mention string

---

### DiscordMemberWrapper.cs

**Implements:** `IDiscordMember`

**Purpose:** Wraps DSharpPlus `DiscordMember`.

**Properties:**

- `Id` - Member ID
- `Username` - Member name
- `Mention` - Discord mention string
- `IsBot` - Whether member is a bot
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
- `Duration` - Track duration
- `ArtworkUri` - Artwork image URI (nullable)
- `StartPosition` - Optional playback start position
- `QueueItemId` - DB queue item ID linked to this track (set when dequeued from `IQueueRepository`; used by `TrackEndedHandlerService` to mark the item as played/skipped)

**Methods:**

- `ToLavalinkTrack()` - Get underlying Lavalink track

---

### DiscordClientEventHandler.cs

**Purpose:** Register and manage Discord client event handlers.

**Methods:**

- `OnClientReady()` - Connect Lavalink when Discord is ready
- `OnGuildAvailable()` - Ensure guild row exists, load localization, and initialize Lavalink guild state

**Features:**

- Handles Discord client startup and guild availability lifecycle hooks

---

### DiscordClientFactory.cs

**Purpose:** Create and configure Discord client instances.

**Methods:**

- `Create()` - Create configured Discord client

**Configuration:**

- Sets intents (privileged and unprivileged)
- Enables auto reconnect
- Registers `Ready` and `GuildAvailable` handlers

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

