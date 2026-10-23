# Validation

Normalizes and validates playlist names consistently across bot and API entry points.

## Overview

- PlaylistNameValidator checks the name before a caller reaches a repository or submits a playlist command.
- Keep the same naming policy across transports. Database uniqueness remains a separate concern even when a name is syntactically valid.

## Classes

### PlaylistNameValidator

**File:** `DC bot.Contracts/Validation/PlaylistNameValidator.cs`

**Purpose:** Normalizes playlist names to 1–64 trimmed characters while rejecting carriage returns and line feeds.

**Methods:**

- `TryNormalize` — Trims and validates playlist names or normalizes supported mobile language codes, returning false for unsupported input.

```csharp
public static bool TryNormalize(string? playlistName, out string normalizedName);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = PlaylistNameValidator.TryNormalize(playlistName, out var normalizedName);
```

---

## Related Components

- `DC bot.Contracts/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
