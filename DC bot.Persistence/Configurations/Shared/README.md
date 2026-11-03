# Shared

Centralizes the Discord guild identifier conversion used by entity mappings.

## Overview

- Feature configuration classes call GuildIdPropertyBuilderExtensions rather than repeating provider conversion rules.
- Keep this helper focused on model mapping; it must not execute repository queries or application commands.

## Classes

### GuildIdPropertyBuilderExtensions

**File:** `DC bot.Persistence/Configurations/Shared/GuildIdPropertyBuilderExtensions.cs`

**Purpose:** Provides the shared PostgreSQL representation for unsigned Discord guild identifiers.

**Methods:**

- `HasGuildIdStorage` — Maps unsigned Discord snowflakes to the shared PostgreSQL storage representation.

```csharp
internal static PropertyBuilder<ulong> HasGuildIdStorage(this PropertyBuilder<ulong> builder);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = builder.HasGuildIdStorage();
```

---

## Related Components

- `DC bot.Persistence/Configurations/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
