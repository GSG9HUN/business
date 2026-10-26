# Guilds

Implements shared storage operations for guild defaults, premium state, and the mapped premium-audit schema.

## Overview

- Callers resolve the contracts through AddPersistenceServices; implementations create contexts through IDbContextFactory and map database state into shared records.
- Forward caller cancellation through database I/O. Preserve transactional ownership and use the shared retry/rollback helpers where a multi-step mutation requires them.
- GuildDataBootstrapper initializes required guild rows. GuildPremiumAuditEntity is mapped in the model, but this folder does not implement a dedicated premium-audit repository.

## Classes

### GuildDataBootstrapper

**File:** `DC bot.Persistence/Repositories/Guilds/GuildDataBootstrapper.cs`

**Purpose:** Ensures guild ownership rows exist within the caller's existing context and transaction.

**Methods:**

- `EnsureExistsAsync` — Creates missing guild or settings state using the owning repository's defaults and unique-conflict policy.

```csharp
internal static async Task EnsureExistsAsync(BotDbContext dbContext, ulong guildId, CancellationToken cancellationToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await GuildDataBootstrapper.EnsureExistsAsync(dbContext, guildId, cancellationToken);
```

---

### GuildDataRepository

**File:** `DC bot.Persistence/Repositories/Guilds/GuildDataRepository.cs`

**Purpose:** Reads and mutates guild data state using short-lived contexts from the injected EF factory.

**Implements / inherits:** `IGuildDataRepository`.

**Methods:**

- `EnsureGuildExistsAsync` — Ensures a default guild row exists while tolerating a concurrent insertion of the same guild.

```csharp
public async Task EnsureGuildExistsAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `IsPremiumAsync` — Checks that premium is enabled and has not expired at the time of the query.

```csharp
public async Task<bool> IsPremiumAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `UpsertPremiumAsync` — Creates or updates the guild premium flags and expiry, recovering a competing insert through the shared conflict policy.

```csharp
public async Task UpsertPremiumAsync(
    ulong guildId,
    bool isPremium,
    DateTimeOffset? premiumUntilUtc,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await guildDataRepository.EnsureGuildExistsAsync(guildId, cancellationToken);
```

---

## Twenty-Four-Seven Preferences

**Methods:**

```csharp
Task<TwentyFourSevenSettingsRecord> GetTwentyFourSevenSettingsAsync(
    ulong guildId,
    CancellationToken cancellationToken = default);

Task SetTwentyFourSevenSettingsAsync(
    ulong guildId,
    bool enabled,
    ulong? voiceChannelId,
    ulong? textChannelId,
    CancellationToken cancellationToken = default);
```

**Usage:**

```csharp
await guildDataRepository.SetTwentyFourSevenSettingsAsync(guildId, true, voiceChannelId, textChannelId, cancellationToken);
var settings = await guildDataRepository.GetTwentyFourSevenSettingsAsync(guildId, cancellationToken);
```

**Validation Checks:**

- Reject guild identifier zero and enabling without nonzero voice and text channel identifiers.
- Return disabled defaults when the guild has not been stored.
- Ensure the guild exists using the existing unique-conflict insertion policy before updating preferences.
- Load the tracked guild entity, assign the relevant properties, and save through `SaveChangesAsync`.
- Disabling preserves preferred channels; preference updates preserve premium fields.
- Forward caller cancellation through database operations.

---

## Related Components

- `DC bot.Persistence/Repositories/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Db/` — Owns the EF Core context, design-time creation, and startup migration runner.
- `DC bot.Persistence/Entities/Guilds/` — Represents the database rows used for guild defaults, premium state, and the mapped premium-audit schema.
- `DC bot.Persistence/Repositories/Shared/` — Provides PostgreSQL transaction retries and owned rollback handling shared by repositories.
- `DC bot.Contracts/Interface/Service/Persistence/Guilds/` — Defines repository operations for guild defaults, premium state, and the mapped premium-audit schema.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
