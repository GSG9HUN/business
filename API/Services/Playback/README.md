# Playback

Resolves plain requester names for mobile playback and queue read models.

## Overview

- Prefer the plain name captured when a track was enqueued.
- Preserve existing plain requester values and resolve legacy `<@id>` or `<@!id>` mentions from stored profiles.
- Use a global name before a username; omit the label if neither is available.
- Reuse profile lookups within the scoped resolver without external Discord requests.

## Components

### RequesterDisplayNameResolver

**Purpose:** Produce display labels for REST and realtime responses without modifying persisted mention metadata.

**Methods:**

- `ResolveAsync` resolves one stored requester and optional captured name.
- `ResolveQueueAsync` copies queue occurrences with plain requester labels while preserving identity and history fields.

**Validation Checks:**

- Unit tests in `RequesterDisplayNameResolverTests` cover captured names, legacy mention forms, malformed identifiers, missing profiles, cancellation, and immutable queue mapping.
- No client-side mention parser or mobile DTO contract change is required.

---

## Related Components

- [Playback snapshots](../../Snapshots/Playback/README.md) - Current-track requester labels.
- [Queue handlers](../../Handlers/Queue/README.md) - REST queue and history labels.
- [Realtime snapshots](../../Realtime/Snapshots/README.md) - Matching realtime requester labels.
