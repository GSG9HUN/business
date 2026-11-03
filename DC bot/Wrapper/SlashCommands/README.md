# SlashCommands

Translates DSharpPlus interaction context and response state into bot-facing message contracts.

## Overview

- The factory creates an interaction context; wrappers expose deferred, initial, and follow-up response operations to the shared command pipeline.
- An interaction response is not an ordinary text-message send. Preserve its response lifecycle instead of making commands know SDK-specific reply rules.

## Classes

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

- `DC bot/Wrapper/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Wrapper/Discord/` — Wraps messages, users, guilds, channels, members, and voice state and handles Discord client events.
- `DC bot/Interface/Service/SlashCommands/` — Connects DSharpPlus interactions to the shared text command pipeline.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
