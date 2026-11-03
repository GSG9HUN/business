# Exceptions

Defines the common exception boundary and feature-specific failure categories.

## Overview

- Services raise typed failures where callers need operation context; command boundaries log failures and produce localized responses.
- Expected validation failures also use result objects. Do not use an exception simply to represent every rejected user input.

## Subfolders

### Localization/

Represents failures while loading or resolving translation resources.

**Files:**

- `LocalizationException.cs`

**Purpose:** LocalizationService supplies context about missing or invalid localization data for logging and caller handling.

---

### Messaging/

Represents Discord response delivery failures at the presentation boundary.

**Files:**

- `MessageSendException.cs`

**Purpose:** Response-building and notification paths can attach message-send context while preserving the underlying cause.

---

### Music/

Distinguishes Lavalink failures, track-load failures, expired sessions, and busy control operations.

**Files:**

- `LavalinkOperationException.cs`
- `PlaybackOperationBusyException.cs`
- `PlaybackSessionExpiredException.cs`
- `TrackLoadException.cs`

**Purpose:** Lifecycle and playback services use these types to prevent stale or competing commands from acting on a replaced player.

---

## Classes

### BotException

**File:** `DC bot/Exceptions/BotException.cs`

**Purpose:** Base exception for all bot-related errors.

**Implements / inherits:** `Exception`.

---

### LocalizationException

**File:** `DC bot/Exceptions/Localization/LocalizationException.cs`

**Purpose:** Exception thrown when localization operations fail.

**Implements / inherits:** `BotException`.

**Properties:**

```csharp
public string LanguageCode { get; }
```

---

### MessageSendException

**File:** `DC bot/Exceptions/Messaging/MessageSendException.cs`

**Purpose:** Exception thrown when Discord message operations fail.

**Implements / inherits:** `BotException`.

**Properties:**

```csharp
public string Operation { get; }
```

---

### LavalinkOperationException

**File:** `DC bot/Exceptions/Music/LavalinkOperationException.cs`

**Purpose:** Exception thrown when Lavalink operations fail.

**Implements / inherits:** `BotException`.

**Properties:**

```csharp
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
public string Query { get; }
```

---

## Related Components

- `DC bot/README.md` — Parent feature overview and neighboring components.
- `DC bot/Exceptions/Localization/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Exceptions/Messaging/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Exceptions/Music/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
