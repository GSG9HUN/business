# Properties

Contains assembly-level metadata for supported internal test access.

## Overview

- AssemblyInfo.cs exposes internal implementation details to the test assembly without making those types public application APIs.
- Keep friend-assembly declarations limited to intentional testing boundaries. This folder does not register runtime services.

## Assembly Metadata

### AssemblyInfo.cs

**File:** `DC bot/Properties/AssemblyInfo.cs`

**Purpose:** Declares friend-assembly access used by the local test project without turning internal helpers into public runtime APIs.

---

## Related Components

- `DC bot/README.md` — Parent feature overview and neighboring components.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
