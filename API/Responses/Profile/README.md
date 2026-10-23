# Profile

Defines client-facing output for mobile user identity and optional preference updates.

## Overview

- Handlers, mappers, and snapshot providers construct these records from shared contract values and return them through HTTP or matching realtime events.
- Keep field meaning consistent for mobile consumers. An output model describes observable state and must not contain a tracked entity or a live SDK object.

## Models and Results

### ProfileResponse

**File:** `API/Responses/Profile/ProfileResponse.cs`

**Purpose:** Carries the profile payload returned to HTTP and mobile clients.

**Record declaration:**

```csharp
public sealed record ProfileResponse(ProfileUserResponse User, ProfileSettingsResponse UserSettings);
```

---

### ProfileSettingsResponse

**File:** `API/Responses/Profile/ProfileSettingsResponse.cs`

**Purpose:** Carries the profile settings payload returned to HTTP and mobile clients.

**Record declaration:**

```csharp
public sealed record ProfileSettingsResponse(
    string LanguageCode,
    string Theme,
    bool HapticFeedbackEnabled,
    bool SoundEffectsEnabled,
    bool TelemetryEnabled,
    DateTimeOffset UpdatedAtUtc);
```

---

### ProfileUserResponse

**File:** `API/Responses/Profile/ProfileUserResponse.cs`

**Purpose:** Carries the profile user payload returned to HTTP and mobile clients.

**Record declaration:**

```csharp
public sealed record ProfileUserResponse(
    string DiscordUserId,
    string Username,
    string? DisplayName,
    string? AvatarUrl,
    bool IsDiscordConnected,
    bool IsActive);
```

---

## Related Components

- `API/Responses/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
