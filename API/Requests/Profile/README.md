# Profile

Defines client-supplied input for mobile user identity and optional preference updates.

## Overview

- Minimal API binding constructs these values; feature filters and handlers normalize and validate them before repository or service calls.
- Optional input can mean leave unchanged rather than reset. Request fields are not authorization evidence; the authenticated principal and stored guild access determine permission.

## Models and Results

### UpdateProfileSettingsRequest

**File:** `API/Requests/Profile/UpdateProfileSettingsRequest.cs`

**Purpose:** Carries client input for update profile settings processing; handlers and endpoint filters validate its values.

**Record declaration:**

```csharp
public sealed record UpdateProfileSettingsRequest(
    string? LanguageCode,
    string? Theme,
    bool? HapticFeedbackEnabled,
    bool? SoundEffectsEnabled,
    bool? TelemetryEnabled);
```

---

## Related Components

- `API/Requests/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
