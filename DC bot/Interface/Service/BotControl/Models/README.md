# Models

Carries resolved Discord execution context and normalized command results within the bot.

## Overview

- The dispatcher and response service use these values to execute an accepted command and persist its outcome.
- These are bot runtime values, not API response records. Store only the explicit serialized command result across process boundaries.

## Models and Results

### BotControlCommandResult

**File:** `DC bot/Interface/Service/BotControl/Models/BotControlCommandResult.cs`

**Purpose:** Contains durable command results and optional Discord-notification metadata.

**Record declaration:**

```csharp
public sealed record BotControlCommandResult(
    bool Success,
    string Message,
    string ResultJson,
    ulong GuildId,
    ulong UserId,
    ulong? VoiceChannelId,
    ulong? TextChannelId,
    bool ShouldNotifyDiscord);
```

---

### BotControlExecutionContext

**File:** `DC bot/Interface/Service/BotControl/Models/BotControlExecutionContext.cs`

**Purpose:** Contains guild, user, channel, and notification permissions resolved for a mobile command.

**Record declaration:**

```csharp
public sealed record BotControlExecutionContext(
    ulong GuildId,
    ulong UserId,
    ulong? VoiceChannelId,
    ulong? TextChannelId,
    bool CanSendDiscordResponse);
```

---

## Related Components

- `DC bot/Interface/Service/BotControl/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
