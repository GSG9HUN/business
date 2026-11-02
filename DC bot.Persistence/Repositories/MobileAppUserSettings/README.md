# MobileAppUserSettings

Implements shared storage operations for per-user mobile language, theme, feedback, sound, and telemetry preferences.

## Overview

- Callers resolve the contracts through AddPersistenceServices; implementations create contexts through IDbContextFactory and map database state into shared records.
- Forward caller cancellation through database I/O. Preserve transactional ownership and use the shared retry/rollback helpers where a multi-step mutation requires them.

## Classes

### MobileAppUserSettingsRepository

**File:** `DC bot.Persistence/Repositories/MobileAppUserSettings/MobileAppUserSettingsRepository.cs`

**Purpose:** Reads and mutates mobile app user settings state using short-lived contexts from the injected EF factory.

**Implements / inherits:** `IMobileAppUserSettingsRepository`.

**Methods:**

- `GetOrCreateAsync` — Reads the guild playback state or mobile-user settings, creating documented defaults when missing.

```csharp
public async Task<MobileAppUserSettingsRecord> GetOrCreateAsync(ulong discordUserId, CancellationToken ct = default);
```

- `EnsureExistsAsync` — Creates missing guild or settings state using the owning repository's defaults and unique-conflict policy.

```csharp
public async Task EnsureExistsAsync(ulong discordUserId, CancellationToken ct = default);
```

- `PatchAsync` — Applies only supplied settings fields, preserving fields omitted from the patch.

```csharp
public async Task<MobileAppUserSettingsRecord> PatchAsync(
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

- `DC bot.Persistence/Repositories/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Db/` — Owns the EF Core context, design-time creation, and startup migration runner.
- `DC bot.Persistence/Entities/MobileAppUserSettings/` — Represents the database rows used for per-user mobile language, theme, feedback, sound, and telemetry preferences.
- `DC bot.Persistence/Repositories/Shared/` — Provides PostgreSQL transaction retries and owned rollback handling shared by repositories.
- `DC bot.Contracts/Interface/Service/Persistence/MobileAppUserSettings/` — Defines repository operations for per-user mobile language, theme, feedback, sound, and telemetry preferences.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
