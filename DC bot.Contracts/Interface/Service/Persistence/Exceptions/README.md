# Exceptions

Defines the implementation-independent error used for a uniqueness conflict.

## Overview

- Repositories translate a provider-specific uniqueness violation into UniqueConstraintConflictException so callers can produce a stable conflict outcome.
- Do not force callers to inspect Npgsql exception types or constraint codes to recognize an expected duplicate.

## Classes

### UniqueConstraintConflictException

**File:** `DC bot.Contracts/Interface/Service/Persistence/Exceptions/UniqueConstraintConflictException.cs`

**Purpose:** Reports a translated unique-storage conflict while retaining the original persistence failure.

**Implements / inherits:** `Exception`.

---

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
