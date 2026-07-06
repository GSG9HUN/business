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
- `Guild` - Parent guild from the channel or an explicitly supplied guild context

**Methods:**

- `SendMessageAsync()` - Send message to channel
- `ToDiscordChannel()` - Get underlying DSharpPlus channel

**Notes:**

- Some DSharpPlus event payloads do not populate `DiscordChannel.Guild`; callers can pass the guild explicitly to keep command and reaction handling guild-aware.
- `Guild` throws if neither the channel nor the explicit constructor argument contains guild context.

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

### SlashInteractionContextFactory.cs

**Implements:** `ISlashInteractionContextFactory`

**Purpose:** Creates `ISlashInteractionContext` wrappers from DSharpPlus `CommandContext` instances. This keeps framework-owned slash contexts behind a testable boundary.

---

### SlashInteractionContextWrapper.cs

**Implements:** `ISlashInteractionContext`

**Purpose:** Adapts DSharpPlus command/slash contexts to the bot's internal slash command abstraction.

**Properties:**

- `GuildId` / `Guild` - Current guild context when the slash command is guild-scoped
- `Channel` - Interaction channel wrapper with explicit guild context
- `User` / `Member` - Interaction user/member wrappers
- `IsDeferred` / `HasResponded` - Response state tracked for slash execution fallback behavior

**Methods:**

- `DeferAsync()` - defer the interaction response
- `RespondAsync()` - send, edit, or follow up interaction responses
- `CreateMessage()` - create an `IDiscordMessage`-compatible command message

---

### SlashInteractionMessageWrapper.cs

**Implements:** `IDiscordMessage`

**Purpose:** Lets slash command execution reuse existing text command implementations.

**Notes:**

- `Content` is built as `commandName` or `commandName argument` so text commands can parse it consistently.
- `RespondAsync()` delegates back to the slash interaction context.
- `ModifyAsync()` responds with builder content or the first embed, matching the subset used by the shared command pipeline.

---

### DiscordClientEventHandler.cs

**Purpose:** Handle Discord gateway, lifecycle, guild, and voice diagnostics.

**Methods:**

- `OnSocketOpened()` - Log gateway socket open events
- `OnSocketClosed()` - Log gateway close events, including critical logging for Discord voice disconnect close code `4014`
- `OnClientReady()` - Log session creation and connect Lavalink when Discord is ready
- `OnSessionResumed()` - Log resumed gateway sessions
- `OnZombied()` - Log zombied gateway state and heartbeat failure details
- `OnGuildAvailable()` - Ensure guild row exists, load localization, and initialize Lavalink guild state
- `OnVoiceStateUpdated()` - Log bot voice-state changes and explicit bot voice disconnects
- `OnVoiceServerUpdated()` - Log voice server endpoint changes without leaking the voice token
- `OnUnknownEvent()` - Log unknown gateway events, with elevated visibility for unknown voice events and truncated payloads

**Features:**

- Uses direct constructor injection for `IGuildDataRepository`, `ILocalizationService`, and `ILavaLinkService`
- Does not resolve dependencies through `IServiceProvider` internally
- Is wired to DSharpPlus 5 event callbacks by `Startup/DependencyInjection/DiscordServiceCollectionExtensions.cs`

---

### DiscordClientFactory.cs

**Purpose:** Create and configure Discord client instances.

**Methods:**

- `Create()` - Create configured Discord client

**Configuration:**

- Creates a default DSharpPlus client with `DiscordIntents.All`
- Configures DSharpPlus logging with minimum `Debug` level for direct/test-created clients
- Does not register event handlers; lifecycle/message/reaction/voice event wiring is configured by `Startup/DependencyInjection/DiscordServiceCollectionExtensions.cs`
- Production startup currently creates the client through DSharpPlus DI builder APIs; this factory remains useful for tests and direct wrapper-level construction

---

## Usage Pattern

Wrappers are created from DSharpPlus objects:

```csharp
// In CommandHandlerService
var wrappedMessage = DiscordMessageWrapperFactory.Create(
    discordMessage, 
    channel, 
    author, 
    logger,
    guild
);

// Pass to command (expects IDiscordMessage interface)
await command.ExecuteAsync(wrappedMessage);
```

## Related Components

- **Interface/Discord/** - Wrapper contracts
- **Service/** - Consumes wrappers
- **Commands/** - Primary wrapper users
- **Exceptions/** - Wrapped exceptions

