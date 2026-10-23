# Constants

Centralizes identifiers used to select localized messages and validation errors.

## Overview

- `LocalizationKeys.TwentyFourSevenCommand*` identifies the description, enabled/disabled status, invalid boolean argument, and settings failure responses.
- Commands and services pass these keys to LocalizationService and ResponseBuilder instead of duplicating translation strings.
- When adding a visible message, update the required keys in both English and Hungarian translation files. Keys identify messages; they are not the translated text.

## Classes

### LocalizationKeys

**File:** `DC bot/Constants/LocalizationKeys.cs`

**Purpose:** Contains all localization keys used throughout the application.

---

### ValidationErrorKeys

**File:** `DC bot/Constants/ValidationErrorKeys.cs`

**Purpose:** Contains all validation error keys used for user and system validation.

---

## Related Components

- `DC bot/README.md` — Parent feature overview and neighboring components.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
