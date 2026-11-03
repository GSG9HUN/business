# Music

Distinguishes Lavalink failures, track-load failures, expired sessions, and busy control operations.

## Overview

- Lifecycle and playback services use these types to prevent stale or competing commands from acting on a replaced player.
- A busy or superseded operation is different from a node outage. Preserve these distinctions when mapping failures to user responses.

## Classes

### LavalinkOperationException

**File:** `DC bot/Exceptions/Music/LavalinkOperationException.cs`

**Purpose:** Exception thrown when Lavalink operations fail.

**Implements / inherits:** `BotException`.

**Properties:**

```csharp
// The operation that failed.
public string Operation { get; }
```

---

### PlaybackOperationBusyException

**File:** `DC bot/Exceptions/Music/PlaybackOperationBusyException.cs`

**Purpose:** Signals a competing or unresolved operation that prevents playback control.

**Implements / inherits:** `InvalidOperationException`.

---

### PlaybackSessionExpiredException

**File:** `DC bot/Exceptions/Music/PlaybackSessionExpiredException.cs`

**Purpose:** Signals that work no longer owns the current guild player generation.

**Implements / inherits:** `InvalidOperationException`.

---

### TrackLoadException

**File:** `DC bot/Exceptions/Music/TrackLoadException.cs`

**Purpose:** Exception thrown when a track cannot be loaded or played.

**Implements / inherits:** `BotException`.

**Properties:**

```csharp
// The search query or track URL.
public string Query { get; }
```

---

## Related Components

- `DC bot/Exceptions/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
