# MobileAppUserSettings

Defines repository operations for per-user mobile language, theme, feedback, sound, and telemetry preferences.

## Overview

- The bot and API consume these interfaces; concrete implementations and DI registration live in DC bot.Persistence unless noted below.
- Describe lookup absence, ordering, conditional mutation, and cancellation in the contract. Keep provider-specific SQL and EF types out of this boundary.

## Interfaces

### IMobileAppUserSettingsRepository

**File:** `DC bot.Contracts/Interface/Service/Persistence/MobileAppUserSettings/IMobileAppUserSettingsRepository.cs`

**Purpose:** Defines shared storage operations for mobile app user settings repository, including caller cancellation and immutable result contracts.

**Methods:**

- `GetOrCreateAsync` — Reads the guild playback state or mobile-user settings, creating documented defaults when missing.

```csharp
Task<MobileAppUserSettingsRecord> GetOrCreateAsync(ulong discordUserId, CancellationToken ct = default);
```

- `EnsureExistsAsync` — Creates missing guild or settings state using the owning repository's defaults and unique-conflict policy.

```csharp
Task EnsureExistsAsync(ulong discordUserId, CancellationToken ct = default);
```

- `PatchAsync` — Applies only supplied settings fields, preserving fields omitted from the patch.

```csharp
Task<MobileAppUserSettingsRecord> PatchAsync(
    ulong discordUserId,
    MobileAppUserSettingsPatchRecord patch,
    CancellationToken ct = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await mobileAppUserSettingsRepository.GetOrCreateAsync(discordUserId, ct);
```

---

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Contracts/Interface/Service/Persistence/Models/MobileAppUserSettings/` — Defines shared records and state values for per-user mobile language, theme, feedback, sound, and telemetry preferences.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
