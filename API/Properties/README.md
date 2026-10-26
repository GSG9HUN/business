# Properties

Contains launch profiles used by local ASP.NET Core development tools.

## Overview

- launchSettings.json supplies local process URLs and launch options. Container binding is configured separately by Docker Compose.
- Launch profiles are developer settings, not a substitute for required OAuth, signing, and database configuration.

## Launch Profiles

**File:** `launchSettings.json`

**Purpose:** Supplies local process profiles and URLs. Container binding comes from Docker Compose instead.

## Related Components

- `API/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
