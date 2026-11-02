# DependencyInjection

Registers the shared PostgreSQL context factory, repositories, queue helpers, and command notifier.

## Overview

- Both executable projects call AddPersistenceServices with their connection string. Singleton repositories create short-lived contexts rather than sharing a tracked DbContext.
- Resolve QueueRepository through IQueueRepository so its claim and batch dependencies are supplied. Keep runtime registration here and design-time context creation in Db.

## Classes

### PersistenceServiceCollectionExtensions

**File:** `DC bot.Persistence/DependencyInjection/PersistenceServiceCollectionExtensions.cs`

**Purpose:** Composes shared PostgreSQL contexts and repository dependencies for the bot and API.

**Methods:**

- `AddPersistenceServices` — Registers PostgreSQL contexts, repository helpers, and shared repository contracts with their runtime lifetimes.

```csharp
public static IServiceCollection AddPersistenceServices(this IServiceCollection services, string postgresConnectionString);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = services.AddPersistenceServices(postgresConnectionString);
```

---

## Related Components

- `DC bot.Persistence/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/Db/` — Owns the EF Core context, design-time creation, and startup migration runner.
- `DC bot.Persistence/Repositories/BotControl/` — Implements shared storage operations for durable bot commands, their claim/completion state, and cross-process change signals.
- `DC bot.Persistence/Repositories/GuildBotStatus/` — Implements shared storage operations for persisted bot voice presence, channel display metadata, and voice-user counts.
- `DC bot.Persistence/Repositories/Guilds/` — Implements shared storage operations for guild defaults, premium state, and the mapped premium-audit schema.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
