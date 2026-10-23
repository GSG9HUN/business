# Playback

Defines client-supplied input for current playback state, repeat flags, start ownership, and canonical batch metadata.

## Overview

- Minimal API binding constructs these values; feature filters and handlers normalize and validate them before repository or service calls.
- Optional input can mean leave unchanged rather than reset. Request fields are not authorization evidence; the authenticated principal and stored guild access determine permission.
- SetRepeatModeRequest uses none, one, or all. The explicit repeat-mode endpoint updates persistence directly and is separate from commands dispatched to the bot worker.

## Models and Results

### SetRepeatModeRequest

**File:** `API/Requests/Playback/SetRepeatModeRequest.cs`

**Purpose:** Carries client input for set repeat mode processing; handlers and endpoint filters validate its values.

**Record declaration:**

```csharp
public sealed record SetRepeatModeRequest(string Mode);
```

---

## Related Components

- `API/Requests/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
