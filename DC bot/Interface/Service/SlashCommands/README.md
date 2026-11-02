# SlashCommands

Connects DSharpPlus interactions to the shared text command pipeline.

## Overview

- Factories create interaction contexts; execution requests describe the text command and argument payload to dispatch.
- The interaction adapter owns defer/respond behavior. Preserve the shared command's validation and service behavior when adding slash options.

## Interfaces

### ISlashCommandExecutor

**File:** `DC bot/Interface/Service/SlashCommands/ISlashCommandExecutor.cs`

**Purpose:** Defines the slash command executor boundary so callers are independent of its concrete implementation.

**Methods:**

- `ExecuteAsync` — Executes the command request through the registered command pipeline and completes its response.

```csharp
Task ExecuteAsync(SlashCommandExecutionRequest request, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await slashCommandExecutor.ExecuteAsync(request, cancellationToken);
```

---

### ISlashInteractionContext

**File:** `DC bot/Interface/Service/SlashCommands/ISlashInteractionContext.cs`

**Purpose:** Defines the slash interaction context boundary so callers are independent of its concrete implementation.

**Properties:**

```csharp
// The Discord guild identifier, or null outside a guild.
ulong? GuildId { get; }
IDiscordGuild? Guild { get; }
IDiscordChannel Channel { get; }
IDiscordUser User { get; }
IDiscordMember? Member { get; }
bool IsDeferred { get; }
bool HasResponded { get; }
```

**Methods:**

- `CreateMessage` — Creates a command-message adapter that sends responses through this slash interaction.

```csharp
IDiscordMessage CreateMessage(string commandName, string? argument = null);
```

- `DeferAsync` — Acknowledges an interaction so its response can be completed after command execution.

```csharp
Task DeferAsync();
```

- `RespondAsync` — Sends a response through the wrapped message or interaction boundary.

```csharp
Task RespondAsync(string message);
```

- `RespondAsync` — Sends a response through the wrapped message or interaction boundary.

```csharp
Task RespondAsync(DiscordEmbed embed);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = slashInteractionContext.CreateMessage(commandName, argument);
```

---

## Factory Classes

### ISlashInteractionContextFactory

**File:** `DC bot/Interface/Service/SlashCommands/ISlashInteractionContextFactory.cs`

**Purpose:** Defines the slash interaction context factory boundary so callers are independent of its concrete implementation.

**Methods:**

- `Create` — Constructs the adapter or runtime object from supplied context and dependencies.

```csharp
ISlashInteractionContext Create(CommandContext context);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = slashInteractionContextFactory.Create(context);
```

---

## Models and Results

### SlashCommandExecutionRequest

**File:** `DC bot/Interface/Service/SlashCommands/SlashCommandExecutionRequest.cs`

**Purpose:** Specifies command arguments and the acknowledgement policy for slash execution.

**Record declaration:**

```csharp
public sealed record SlashCommandExecutionRequest(
    string CommandName,
    ISlashInteractionContext Context,
    string? Argument = null,
    bool RequireGuild = false,
    bool Defer = false,
    bool EnsureDeferredResponse = false);
```

---

## Related Components

- `DC bot/Interface/Service/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
