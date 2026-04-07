# guildFiles

This folder stores filesystem-based per-guild data.

Queue persistence is now database-backed and no longer stored here by the active queue repository implementation.

## Structure

```
guildFiles/
├── localization/
│   └── {guildId}.json        # Guild language preference
└── queues/
  └── legacy files          # Legacy queue snapshots (not active DB path)
```

## Subfolders

### localization/

**Purpose:** Store guild language preferences.

**Format:**

```json
{
  "language": "en"
}
```

**File:** `{guildId}.json`

**Usage:**

```csharp
// Save guild preference
localizationService.SaveLanguage(guildId, "hu");

// Load guild preference
localizationService.LoadLanguage(guildId);
```

---

### queues/

**Purpose:** Legacy queue persistence location.

**Status:**

- Current queue persistence uses PostgreSQL via `IQueueRepository`.
- Existing files in this folder can be treated as historical/legacy artifacts.

---

## File Management

- **Created automatically** - Directories created on first write
- **Per-guild isolation** - Each guild has separate files
- **Runtime managed** - Don't edit manually while bot runs
- **Corruption handling** - Invalid JSON falls back to defaults

---

## Related Components

- **Service/LocalizationService.cs** - Language preference management
- **Interface/Service/IO/IFileSystem.cs** - Filesystem operations
- **Interface/Service/Persistence/IQueueRepository.cs** - Active queue persistence contract
- **Persistence/README.md** - Database persistence overview

