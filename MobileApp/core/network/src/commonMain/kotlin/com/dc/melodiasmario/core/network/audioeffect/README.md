# audioeffect

HTTP operations, transport DTOs and remote mapping for guild audio effects and presets.

## Overview

The explicit `saveAudioEffectPreset` operation persists the draft through POST `/guilds/{guildId}/playback/audio-effects/presets`. It does not use `applyAudioEffects` and never applies live settings. The save request carries the preset name, setting edits and overwrite consent; the existing command coordinator still waits for terminal completion.

This package is in `commonMain` and depends on `core:model` and `core:common`; it does not depend on domain or UI.

`AudioEffectApiService` sends GET/POST requests to `/guilds/{guildId}/playback/audio-effects`. `AudioEffectRemoteDataSourceImpl` maps DTOs to shared models. Command acceptance is separate from terminal bot completion. Koin discovers the service and data source through the existing network component scan.

## Subfolders

### dto/

Serializable request and response types.

**Files:**

- `dto/AudioEffectsDto.kt`
- `dto/AudioEffectsRequestDto.kt`
- `dto/SaveAudioEffectPresetRequestDto.kt`

**Purpose:** Transports server data without exposing HTTP details to the domain.

---

## Components

### AudioEffectApiService

**Purpose:** Sends authenticated audio-effect requests.

---

### AudioEffectRemoteDataSource

**Purpose:** Defines remote audio-effect access.

---

### AudioEffectRemoteDataSourceImpl

**Purpose:** Maps remote results to shared models.

---

## Related Components

- `core:network` - HTTP transport and remote data sources.
- `core:domain` - Repository contracts and use cases.
- `core:data` - Authorized repository implementations.
- `core:model` - Shared platform-independent values.
- `feature:audioeffect:presentation` - Draft editing and command completion.
