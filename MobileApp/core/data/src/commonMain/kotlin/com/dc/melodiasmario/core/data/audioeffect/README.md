# audioeffect

Audio-effect repository implementation backed by the remote data source and authorized sessions.

## Overview

The explicit `saveAudioEffectPreset` operation persists the draft through POST `/guilds/{guildId}/playback/audio-effects/presets`. It does not use `applyAudioEffects` and never applies live settings. The save request carries the preset name, setting edits and overwrite consent; the existing command coordinator still waits for terminal completion.

This package is in `commonMain` and implements `core:domain` contracts using `core:network`; it does not depend on feature UI.

`AudioEffectRepositoryImpl` obtains a valid access token before delegating to `AudioEffectRemoteDataSource`. Koin binds it to `AudioEffectRepository` through the existing data component scan. Playback controls remain in the current-track repository.

## Components

### AudioEffectRepositoryImpl

**Purpose:** Coordinates authorization and remote access.

---

## Related Components

- `core:network` - HTTP transport and remote data sources.
- `core:domain` - Repository contracts and use cases.
- `core:data` - Authorized repository implementations.
- `core:model` - Shared platform-independent values.
- `feature:audioeffect:presentation` - Draft editing and command completion.
