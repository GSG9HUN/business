# Properties

Controls test access to internal persistence helpers.

## Overview

- AssemblyInfo.cs exposes selected internal code to the test assembly through a friend-assembly declaration.
- Keep internal rollback and retry helpers out of the public storage API; production callers should use repository contracts and DI registration.

## Assembly Metadata

### AssemblyInfo.cs

**File:** `DC bot.Persistence/Properties/AssemblyInfo.cs`

**Purpose:** Declares friend-assembly access used by the local test project without turning internal helpers into public runtime APIs.

---

## Related Components

- `DC bot.Persistence/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
