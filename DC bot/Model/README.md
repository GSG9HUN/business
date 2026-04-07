# Model

This folder contains lightweight domain model classes used by the application.

## Files

### SerializedTrack.cs

**Purpose:** Minimal track identity model.

**Definition:**

```csharp
public class SerializedTrack
{
    public string Identifier { get; init; } = string.Empty;
}
```

**Notes:**

- The queue persistence path is now database-based through repositories.
- This type is intentionally minimal and stores only the Lavalink track identifier.

## Related Components

- `Service/Music/MusicServices/MusicQueueService.cs`
- `Interface/Service/Persistence/README.md`
- `Persistence/README.md`

