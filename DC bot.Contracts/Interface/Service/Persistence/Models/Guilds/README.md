# Guilds

Contains immutable guild preference records shared by persistence and the bot runtime.

## Overview

- Represent the saved twenty-four-seven voice mode without exposing EF entities or Discord SDK objects.
- Return disabled defaults with null channels when no guild row exists.
- Keep desired channel preferences separate from observed voice connection status.

## Preference Records

### TwentyFourSevenSettingsRecord

**File:** `TwentyFourSevenSettingsRecord.cs`

**Purpose:** Carries the enabled flag and preferred voice and notification channel identifiers.

**Properties:**

```csharp
public bool IsEnabled { get; init; }
public ulong? VoiceChannelId { get; init; }
public ulong? TextChannelId { get; init; }
```

**Usage:**

```csharp
var settings = await guildDataRepository.GetTwentyFourSevenSettingsAsync(guildId, cancellationToken);
if (settings.IsEnabled)
{
    var voiceChannelId = settings.VoiceChannelId;
}
```

**Validation Checks:**

- Enabling requires nonzero voice and text channel identifiers.
- Disabling preserves the previously saved channels.
- This record describes preferences; it does not confirm a live voice connection or restore playback.

---

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/Guilds/` - Defines preference reads and writes.
- `DC bot.Persistence/Repositories/Guilds/` - Maps stored guild fields to this record.
- `DC bot/Service/Music/Lifecycle/PersistentVoice/TwentyFourSevenService.cs` - Coordinates command settings and initial voice setup.
