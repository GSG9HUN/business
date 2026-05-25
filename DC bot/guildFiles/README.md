# guildFiles

This folder stores filesystem-based per-guild data.

Queue persistence is now database-backed and no longer stored here by the active queue repository implementation.

## Structure

```
guildFiles/
├── localization/
│   └── {guildId}.json        # Guild language preference
```

## Subfolders

### localization/

**Purpose:** Store guild language preferences.

**Format:**

```json
"eng"
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

### queues/ (legacy only)

**Purpose:** Legacy queue persistence location.

**Status:**

- Current queue persistence uses PostgreSQL via `IQueueRepository`.
- The active source tree does not require this folder.
- If a `queues/` folder exists in an older checkout, its files can be treated as historical/legacy artifacts.

---

## File Management

- **Created automatically** - Directories created on first write
- **Per-guild isolation** - Each guild has separate files
- **Runtime managed** - Don't edit manually while bot runs
- **Corruption handling** - Missing files fall back to defaults; invalid JSON raises `LocalizationException`

---

## Related Components

- **Service/LocalizationService.cs** - Language preference management
- **Interface/Service/IO/IFileSystem.cs** - Filesystem operations
- **Interface/Service/Persistence/IQueueRepository.cs** - Active queue persistence contract
- **Persistence/README.md** - Database persistence overview

