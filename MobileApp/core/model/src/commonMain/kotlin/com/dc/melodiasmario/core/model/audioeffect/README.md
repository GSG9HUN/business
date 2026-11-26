# audioeffect

Shared SDK-independent confirmed audio-effect settings and paged preset metadata.

## Overview

This package is in `commonMain` and has no dependency on network, repositories or UI.

`AudioEffectsSettings` stores speed, pitch, 15 equalizer gains and additional filter parameters. `AudioEffectsSnapshot` adds nullable confirmed settings, preset names, management permission and pagination. Null settings mean that the server has no confirmed filter snapshot.

## Components

### AudioEffectsSettings

**Purpose:** Stores immutable effect parameters.

---

### AudioEffectsSnapshot

**Purpose:** Stores confirmed settings and paged preset metadata.

---

## Related Components

- `core:network` - HTTP transport and remote data sources.
- `core:domain` - Repository contracts and use cases.
- `core:data` - Authorized repository implementations.
- `core:model` - Shared platform-independent values.
- `feature:audioeffect:presentation` - Draft editing and command completion.
