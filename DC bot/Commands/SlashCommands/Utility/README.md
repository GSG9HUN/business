# Utility

Exposes help, language selection, latency checks, and user tagging through slash commands.

## Overview

- DSharpPlus binds interaction options, then each adapter dispatches a SlashCommandExecutionRequest to the shared text command pipeline.
- Keep user input and response presentation here. Playback ownership, track transport, and database transitions belong to the services reached by the command.

## Classes

### HelpSlashCommand

**File:** `DC bot/Commands/SlashCommands/Utility/HelpSlashCommand.cs`

**Purpose:** Adapts the help slash command to shared command execution and interaction acknowledgement.

**Methods:**

- `ExecuteAsync` — Routes the slash interaction through the shared command executor and acknowledgement policy.

```csharp
public Task ExecuteAsync(ISlashInteractionContext context);
```

- `Help` — Adapts the help slash route to its shared handler and interaction response policy.

```csharp
public Task Help(SlashCommandContext context);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await helpSlashCommand.ExecuteAsync(context);
```

---

### LanguageSlashCommand

**File:** `DC bot/Commands/SlashCommands/Utility/LanguageSlashCommand.cs`

**Purpose:** Adapts the slash language-selection interaction to the shared language command and its localized response path.

**Related enum:**

```csharp
public enum SlashLanguage
{
    [ChoiceDisplayName("en")]
    Eng,
    [ChoiceDisplayName("hu")]
    Hu
}
```

**Methods:**

- `ExecuteAsync` — Routes the slash interaction through the shared command executor and acknowledgement policy.

```csharp
public Task ExecuteAsync(ISlashInteractionContext context, SlashLanguage language);
```

- `Language` — Adapts the language slash route to its shared handler and interaction response policy.

```csharp
public Task Language(SlashCommandContext context, [Parameter("language")][Description("Language to use")] SlashLanguage language);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await languageSlashCommand.ExecuteAsync(context, language);
```

---

### PingSlashCommand

**File:** `DC bot/Commands/SlashCommands/Utility/PingSlashCommand.cs`

**Purpose:** Adapts the ping slash command to shared command execution and interaction acknowledgement.

**Methods:**

- `ExecuteAsync` — Routes the slash interaction through the shared command executor and acknowledgement policy.

```csharp
public Task ExecuteAsync(ISlashInteractionContext context);
```

- `Ping` — Adapts the ping slash route to its shared handler and interaction response policy.

```csharp
public Task Ping(SlashCommandContext context);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await pingSlashCommand.ExecuteAsync(context);
```

---

### TagSlashCommand

**File:** `DC bot/Commands/SlashCommands/Utility/TagSlashCommand.cs`

**Purpose:** Adapts the tag slash command to shared command execution and interaction acknowledgement.

**Methods:**

- `Tag` — Adapts the tag slash route to its shared handler and interaction response policy.

```csharp
public Task Tag(SlashCommandContext context, [Parameter("user")][Description("Member you want to tag.")] DiscordMember member);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await tagSlashCommand.Tag(context, member);
```

---

## Related Components

- `DC bot/Commands/SlashCommands/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Interface/Service/SlashCommands/` — Connects DSharpPlus interactions to the shared text command pipeline.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
