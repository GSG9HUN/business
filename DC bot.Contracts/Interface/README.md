# Interface

Groups contracts consumed across project boundaries.

## Overview

- Defines shared repository and external-facing operation contracts.
- Separates consumer boundaries from persistence and bot runtime implementations.

## Subfolders

### Service/

Groups persistence abstractions used by application services and handlers.

**Files:**

- `README.md` — Folder overview and navigation to its source components.

**Purpose:** Feature interfaces expose immutable records and cancellable operations; the persistence project supplies database implementations.

---

## Related Components

- `DC bot.Contracts/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/Interface/Service/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
