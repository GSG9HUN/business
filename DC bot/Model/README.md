# Model

This folder contains data model classes.

## Files

### SerializedTrack.cs

**Purpose:** Persist music tracks to disk.

**Definition:**

```csharp
public class SerializedTrack
{
    public string Title { get; set; }
    public string Author { get; set; }
    public string Uri { get; set; }
    public string Duration { get; set; }  // ISO 8601 format
}
```

**Usage:**

```csharp
// In MusicQueueService
var serialized = new SerializedTrack
{
    Title = track.Title,
    Author = track.Author,
    Uri = track.Uri.ToString(),
    Duration = track.Duration.ToString()
};

// Serialize to JSON
var json = JsonSerializer.Serialize(serialized);
await fileSystem.WriteAllText(filePath, json);
```

**Persistence:**

- Stored in `guildFiles/queues/{guildId}.json`
- Allows queue restoration on bot restart
- Contains only essential track information

---

## Related Components

- **Service/Music/MusicServices/MusicQueueService.cs** - Uses for persistence
- **guildFiles/queues/** - Storage location
- **Interface/Service/IO/IFileSystem.cs** - File operations

