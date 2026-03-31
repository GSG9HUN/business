# guildFiles

This folder stores per-guild persisted data.

## Structure

```
guildFiles/
├── localization/
│   └── {guildId}.json        # Guild language preference
└── queues/
    └── {guildId}.json        # Guild music queue
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

**Purpose:** Persist music queue per guild.

**Format:**

```json
[
  {
    "title": "Track Title",
    "author": "Artist Name",
    "uri": "https://...",
    "duration": "PT3M45S"
  },
  ...
]
```

**File:** `{guildId}.json`

**Usage:**

```csharp
// Save queue
musicQueueService.SaveQueue(guildId);

// Load queue
musicQueueService.LoadQueue(guildId);
```

---

## File Management

- **Created automatically** - Directories created on first write
- **Per-guild isolation** - Each guild has separate files
- **Runtime managed** - Don't edit manually while bot runs
- **Corruption handling** - Invalid JSON falls back to defaults

---

## Related Components

- **Service/LocalizationService.cs** - Language preference management
- **Service/Music/MusicServices/MusicQueueService.cs** - Queue persistence
- **Interface/Service/IO/IFileSystem.cs** - File operations
- **Model/SerializedTrack.cs** - Queue data model

