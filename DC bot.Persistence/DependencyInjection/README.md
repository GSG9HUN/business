# DependencyInjection

Registers the shared PostgreSQL context factory, repositories, queue helpers, and command notifier.

## Overview

- Registers shared database contexts and repository implementations.
- Provides the same persistence boundaries to the bot and API without referencing bot runtime services.

## Classes

### PersistenceServiceCollectionExtensions

**File:** `DC bot.Persistence/DependencyInjection/PersistenceServiceCollectionExtensions.cs`

**Purpose:** Composes shared PostgreSQL contexts and repository dependencies for the bot and API.

Registers favorite, discovery-settings, filter-preset and guild music-control repositories alongside guild, queue and playlist persistence through the shared context factory.

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
