# Messaging

Represents Discord response delivery failures at the presentation boundary.

## Overview

- Response-building and notification paths can attach message-send context while preserving the underlying cause.
- Message delivery failure does not automatically mean a playback transport operation failed; handle the two outcomes separately.

## Classes

### MessageSendException

**File:** `DC bot/Exceptions/Messaging/MessageSendException.cs`

**Purpose:** Exception thrown when Discord message operations fail.

**Implements / inherits:** `BotException`.

**Properties:**

```csharp
// The operation that failed.
public string Operation { get; }
```

---

## Related Components

- `DC bot/Exceptions/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
