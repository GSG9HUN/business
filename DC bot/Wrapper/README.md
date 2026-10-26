# Wrapper

Groups Discord, Lavalink, and slash-interaction implementations of bot-facing contracts.

## Overview

- The Discord wrappers expose SDK objects through application interfaces; Lavalink wrappers adapt track metadata; slash wrappers translate interaction responses.
- Keep SDK naming and fixed signatures where required. Application use cases belong in services, not in wrapper property accessors.

## Subfolders

### Discord/

Wraps messages, users, guilds, channels, members, and voice state and handles Discord client events.

**Files:**

- `DiscordChannelWrapper.cs`
- `DiscordClientEventHandler.cs`
- `DiscordClientFactory.cs`
- `DiscordConnectionEventLogger.cs`
- `DiscordGuildWrapper.cs`
- `DiscordMemberWrapper.cs`
- `DiscordMessageWrapper.cs`
- `DiscordUserWrapper.cs`
- `DiscordVoiceEventLogger.cs`
- `DiscordVoiceStateWrapper.cs`

**Purpose:** DiscordClientEventHandler routes connection and voice events to status persistence and playback recovery. Separate loggers keep event diagnostics focused.

---

### Lavalink/

Exposes Lavalink track metadata through ILavaLinkTrack.

**Files:**

- `LavaLinkTrackWrapper.cs`

**Purpose:** LavaLinkTrackWrapper allows command and presentation code to consume track values without constructing a concrete SDK track.

---

### SlashCommands/

Translates DSharpPlus interaction context and response state into bot-facing message contracts.

**Files:**

- `SlashInteractionContextFactory.cs`
- `SlashInteractionContextWrapper.cs`
- `SlashInteractionMessageWrapper.cs`

**Purpose:** The factory creates an interaction context; wrappers expose deferred, initial, and follow-up response operations to the shared command pipeline.

---

## Classes

### DiscordChannelWrapper

**File:** `DC bot/Wrapper/Discord/DiscordChannelWrapper.cs`

**Purpose:** Adapts discord channel data and operations to the bot's Discord boundary.

**Implements / inherits:** `IDiscordChannel`.

**Properties:**

```csharp
public ulong Id { get; }
public string Name { get; }
public IDiscordGuild Guild { get; }
```

**Methods:**

- `SendMessageAsync` — Sends a message through the wrapped Discord channel.

```csharp
public async Task SendMessageAsync(DiscordEmbed embed);
```

- `SendMessageAsync` — Sends a message through the wrapped Discord channel.

```csharp
public async Task SendMessageAsync(string message);
```

- `ToDiscordChannel` — Returns the underlying DSharpPlus channel represented by the adapter.

```csharp
public DiscordChannel ToDiscordChannel();
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await discordChannelWrapper.SendMessageAsync(embed);
```

---

### DiscordClientEventHandler

**File:** `DC bot/Wrapper/Discord/DiscordClientEventHandler.cs`

**Purpose:** Isolates event failures while maintaining gateway recovery and persisted bot voice state.

**Methods:**

- `OnClientReady` — Processes the Discord client ready event inside the common event-failure boundary.

```csharp
public async Task OnClientReady(DiscordClient sender, SessionCreatedEventArgs? e);
```

- `OnGuildAvailable` — Processes the Discord guild available event inside the common event-failure boundary.

```csharp
public async Task OnGuildAvailable(DiscordClient sender, GuildAvailableEventArgs e);
```

- `OnSessionResumed` — Processes the Discord session resumed event inside the common event-failure boundary.

```csharp
public Task OnSessionResumed(DiscordClient sender, SessionResumedEventArgs e);
```

- `OnSocketClosed` — Processes the Discord socket closed event inside the common event-failure boundary.

```csharp
public Task OnSocketClosed(DiscordClient sender, SocketClosedEventArgs e);
```

- `OnSocketOpened` — Processes the Discord socket opened event inside the common event-failure boundary.

```csharp
public Task OnSocketOpened(DiscordClient sender, SocketOpenedEventArgs e);
```

- `OnUnknownEvent` — Processes the Discord unknown event event inside the common event-failure boundary.

```csharp
public Task OnUnknownEvent(DiscordClient sender, UnknownEventArgs e);
```

**Additional methods:** `OnZombied`, `OnVoiceStateUpdated`, `OnVoiceServerUpdated`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await discordClientEventHandler.OnClientReady(sender, e);
```

---

### DiscordConnectionEventLogger

**File:** `DC bot/Wrapper/Discord/DiscordConnectionEventLogger.cs`

**Purpose:** Records gateway and socket diagnostics with bounded unknown-event payloads.

**Methods:**

- `LogClientReady` — Records client ready diagnostics using available Discord event state.

```csharp
internal void LogClientReady(SessionCreatedEventArgs? e);
```

- `LogSessionResumed` — Records session resumed diagnostics using available Discord event state.

```csharp
internal void LogSessionResumed(SessionResumedEventArgs e);
```

- `LogSocketClosed` — Records socket closed diagnostics using available Discord event state.

```csharp
internal void LogSocketClosed(SocketClosedEventArgs e);
```

- `LogSocketOpened` — Records socket opened diagnostics using available Discord event state.

```csharp
internal void LogSocketOpened();
```

- `LogUnknownEvent` — Records unknown event diagnostics using available Discord event state.

```csharp
internal void LogUnknownEvent(UnknownEventArgs e);
```

- `LogZombied` — Records zombied diagnostics using available Discord event state.

```csharp
internal void LogZombied(ZombiedEventArgs e);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
discordConnectionEventLogger.LogClientReady(e);
```

---

### DiscordGuildWrapper

**File:** `DC bot/Wrapper/Discord/DiscordGuildWrapper.cs`

**Purpose:** Adapts discord guild data and operations to the bot's Discord boundary.

**Implements / inherits:** `IDiscordGuild`.

**Properties:**

```csharp
public ulong Id { get; }
public string Name { get; }
```

**Methods:**

- `GetAllMembersAsync` — Retrieves guild members through the wrapped Discord API.

```csharp
public async Task<IReadOnlyCollection<IDiscordMember>> GetAllMembersAsync();
```

- `GetMemberAsync` — Resolves a guild member through the wrapped Discord API.

```csharp
public async Task<IDiscordMember> GetMemberAsync(ulong id);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await discordGuildWrapper.GetAllMembersAsync();
```

---

### DiscordMemberWrapper

**File:** `DC bot/Wrapper/Discord/DiscordMemberWrapper.cs`

**Purpose:** Adapts discord member data and operations to the bot's Discord boundary.

**Implements / inherits:** `IDiscordMember`.

**Properties:**

```csharp
public ulong Id { get; }
public bool IsBot { get; }
public string Username { get; }
public string Mention { get; }
public IDiscordVoiceState VoiceState { get; }
```

**Methods:**

- `ToDiscordMember` — Returns the underlying DSharpPlus member represented by the adapter.

```csharp
public DiscordMember ToDiscordMember();
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = discordMemberWrapper.ToDiscordMember();
```

---

### DiscordMessageWrapper

**File:** `DC bot/Wrapper/Discord/DiscordMessageWrapper.cs`

**Purpose:** Adapts discord message data and operations to the bot's Discord boundary.

**Implements / inherits:** `IDiscordMessage`.

**Properties:**

```csharp
public ulong Id { get; set; }
public string Content { get; set; }
public IDiscordChannel Channel { get; set; }
public IDiscordUser Author { get; set; }
public DateTimeOffset CreatedAt { get; set; }
public IReadOnlyList<DiscordEmbed> Embeds { get; set; }
```

**Methods:**

- `ModifyAsync` — Updates the wrapped Discord message or interaction response.

```csharp
public async Task ModifyAsync(DiscordMessageBuilder builder);
```

- `RespondAsync` — Sends a response through the wrapped message or interaction boundary.

```csharp
public async Task RespondAsync(string message);
```

- `RespondAsync` — Sends a response through the wrapped message or interaction boundary.

```csharp
public async Task RespondAsync(DiscordEmbed embed);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await discordMessageWrapper.ModifyAsync(builder);
```

---

### DiscordUserWrapper

**File:** `DC bot/Wrapper/Discord/DiscordUserWrapper.cs`

**Purpose:** Adapts discord user data and operations to the bot's Discord boundary.

**Implements / inherits:** `IDiscordUser`.

**Properties:**

```csharp
public ulong Id { get; }
public bool IsBot { get; }
public string Username { get; }
public string Mention { get; }
```

---

### DiscordVoiceEventLogger

**File:** `DC bot/Wrapper/Discord/DiscordVoiceEventLogger.cs`

**Purpose:** Records voice changes while tolerating missing event entities.

**Methods:**

- `LogVoiceServerUpdated` — Records voice server updated diagnostics using available Discord event state.

```csharp
internal void LogVoiceServerUpdated(VoiceServerUpdatedEventArgs e);
```

- `LogVoiceStateUpdated` — Records voice state updated diagnostics using available Discord event state.

```csharp
internal void LogVoiceStateUpdated(DiscordClient sender, VoiceStateUpdatedEventArgs e);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
discordVoiceEventLogger.LogVoiceServerUpdated(e);
```

---

### DiscordVoiceStateWrapper

**File:** `DC bot/Wrapper/Discord/DiscordVoiceStateWrapper.cs`

**Purpose:** Adapts discord voice state data and operations to the bot's Discord boundary.

**Implements / inherits:** `IDiscordVoiceState`.

**Properties:**

```csharp
public IDiscordChannel? Channel
{
    get
    {
        if (discordVoiceState?.ChannelId is null or 0)
        {
            return null;
        }

        var channel = discordVoiceState.GetChannelAsync()
            .GetAwaiter()
            .GetResult();
        return channel is null ? null : new DiscordChannelWrapper(channel);
    }
}
```

**Methods:**

- `ToDiscordVoiceState` — Returns the underlying DSharpPlus voice state represented by the adapter.

```csharp
public DiscordVoiceState ToDiscordVoiceState();
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = discordVoiceStateWrapper.ToDiscordVoiceState();
```

---

### LavaLinkTrackWrapper

**File:** `DC bot/Wrapper/Lavalink/LavaLinkTrackWrapper.cs`

**Purpose:** Adapts lava link track data and operations to the bot's Discord boundary.

**Implements / inherits:** `ILavaLinkTrack`.

**Properties:**

```csharp
public string Title { get; }
public string Author { get; }
public TimeSpan Duration { get; }
public Uri? ArtworkUri { get; }
public TimeSpan? StartPosition { get; }
public long? QueueItemId { get; init; }
```

**Methods:**

- `ToLavalinkTrack` — Returns the underlying SDK track represented by this adapter.

```csharp
public LavalinkTrack ToLavalinkTrack();
```

- `ToString` — Returns the underlying encoded identity or display representation of this adapter.

```csharp
public override string ToString();
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = lavaLinkTrackWrapper.ToLavalinkTrack();
```

---

### SlashInteractionContextWrapper

**File:** `DC bot/Wrapper/SlashCommands/SlashInteractionContextWrapper.cs`

**Purpose:** Adapts slash interaction context data and operations to the bot's Discord boundary.

**Implements / inherits:** `ISlashInteractionContext`.

**Properties:**

```csharp
public ulong? GuildId { get; }
public IDiscordGuild? Guild { get; }
public IDiscordChannel Channel { get; }
public IDiscordUser User { get; }
public IDiscordMember? Member { get; }
public bool IsDeferred { get; private set; }
public bool HasResponded { get; private set; }
```

**Methods:**

- `CreateMessage` — Creates a synthetic command message whose responses are delivered through the wrapped slash interaction.

```csharp
public IDiscordMessage CreateMessage(string commandName, string? argument = null);
```

- `DeferAsync` — Acknowledges an interaction so its response can be completed after command execution.

```csharp
public async Task DeferAsync();
```

- `RespondAsync` — Sends a response through the wrapped message or interaction boundary.

```csharp
public async Task RespondAsync(string message);
```

- `RespondAsync` — Sends a response through the wrapped message or interaction boundary.

```csharp
public async Task RespondAsync(DiscordEmbed embed);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = slashInteractionContextWrapper.CreateMessage(commandName, argument);
```

---

### SlashInteractionMessageWrapper

**File:** `DC bot/Wrapper/SlashCommands/SlashInteractionMessageWrapper.cs`

**Purpose:** Adapts slash interaction message data and operations to the bot's Discord boundary.

**Implements / inherits:** `IDiscordMessage`.

**Properties:**

```csharp
public ulong Id { get; set; }
public string Content { get; set; }
public IDiscordChannel Channel { get; set; }
public IDiscordUser Author { get; set; }
public DateTimeOffset CreatedAt { get; set; }
public IReadOnlyList<DiscordEmbed> Embeds { get; set; }
```

**Methods:**

- `ModifyAsync` — Updates the wrapped Discord message or interaction response.

```csharp
public async Task ModifyAsync(DiscordMessageBuilder builder);
```

- `RespondAsync` — Sends a response through the wrapped message or interaction boundary.

```csharp
public Task RespondAsync(string message);
```

- `RespondAsync` — Sends a response through the wrapped message or interaction boundary.

```csharp
public Task RespondAsync(DiscordEmbed message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await slashInteractionMessageWrapper.ModifyAsync(builder);
```

---

## Factory Classes

### DiscordClientFactory

**File:** `DC bot/Wrapper/Discord/DiscordClientFactory.cs`

**Purpose:** Creates the Discord client with configured credentials, intents, and event handling.

**Methods:**

- `Create` — Constructs the adapter or runtime object from supplied context and dependencies.

```csharp
public static DiscordClient Create(BotSettings settings);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = DiscordClientFactory.Create(settings);
```

---

### SlashInteractionContextFactory

**File:** `DC bot/Wrapper/SlashCommands/SlashInteractionContextFactory.cs`

**Purpose:** Represents the slash interaction context factory boundary and its associated operation data.

**Implements / inherits:** `ISlashInteractionContextFactory`.

**Methods:**

- `Create` — Constructs the adapter or runtime object from supplied context and dependencies.

```csharp
public ISlashInteractionContext Create(CommandContext context);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = slashInteractionContextFactory.Create(context);
```

---

## Related Components

- `DC bot/README.md` — Parent feature overview and neighboring components.
- `DC bot/Wrapper/Discord/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Wrapper/Lavalink/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Wrapper/SlashCommands/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
