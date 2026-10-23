# Guilds

Defines repository operations for guild defaults, premium state, and the mapped premium-audit schema.

## Overview

- The bot and API consume these interfaces; concrete implementations and DI registration live in DC bot.Persistence unless noted below.
- Describe lookup absence, ordering, conditional mutation, and cancellation in the contract. Keep provider-specific SQL and EF types out of this boundary.

## Interfaces

### IGuildDataRepository

**File:** `DC bot.Contracts/Interface/Service/Persistence/Guilds/IGuildDataRepository.cs`

**Purpose:** Defines shared storage operations for guild data repository, including caller cancellation and immutable result contracts.

**Methods:**

- `EnsureGuildExistsAsync` — Ensures a default guild row exists while tolerating a concurrent insertion of the same guild.

```csharp
Task EnsureGuildExistsAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `IsPremiumAsync` — Checks that premium is enabled and has not expired at the time of the query.

```csharp
Task<bool> IsPremiumAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `UpsertPremiumAsync` — Creates or updates the guild premium flags and expiry, recovering a competing insert through the shared conflict policy.

```csharp
Task UpsertPremiumAsync(
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

- `DC bot.Contracts/Interface/Service/Persistence/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
