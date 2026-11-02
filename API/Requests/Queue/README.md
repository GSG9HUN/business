# Queue

Defines client-supplied input for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.

## Overview

- Minimal API binding constructs these values; feature filters and handlers normalize and validate them before repository or service calls.
- Optional input can mean leave unchanged rather than reset. Request fields are not authorization evidence; the authenticated principal and stored guild access determine permission.

## Classes

### EnqueueRequest

**File:** `API/Requests/Queue/EnqueueRequest.cs`

**Purpose:** Carries client input for enqueue processing; handlers and endpoint filters validate its values.

**Properties:**

```csharp
// The search query or track URL.
public string Query { get; init; }
public string? SearchMode { get; init; }
public string? VoiceChannelId { get; init; }
public string? TextChannelId { get; init; }
```

**Methods:**

- `Deconstruct` — Exposes the request values through tuple-style deconstruction without validation or normalization.

```csharp
public void Deconstruct(out string query, out string? searchMode);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
enqueueRequest.Deconstruct(out var query, out var searchMode);
```

---

## Related Components

- `API/Requests/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
