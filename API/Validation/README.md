# Validation

Checks route identifiers, command aliases, and provider-aware enqueue input before handlers run.

## Overview

- GuildIdValidationFilter and CommandNameValidationFilter store normalized route values in HttpContext.Items. MusicEnqueueValidationFilter checks query shape and deployed provider enqueue permission.
- IEndpointFilter requires ValueTask-returning SDK signatures. Route validation does not replace authentication or stored guild-access checks in handlers.

## Classes

### CommandNameValidationFilter

**File:** `API/Validation/CommandNameValidationFilter.cs`

**Purpose:** Normalizes supported playback command route values before invoking handlers.

**Implements / inherits:** `IEndpointFilter`.

**Methods:**

- `InvokeAsync` — Rejects unsupported command names or stores the normalized command before calling the next filter.

```csharp
public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await commandNameValidationFilter.InvokeAsync(context, next);
```

---

### GuildIdValidationFilter

**File:** `API/Validation/GuildIdValidationFilter.cs`

**Purpose:** Validates a nonzero Discord guild route identifier and stores its parsed value for handlers.

**Implements / inherits:** `IEndpointFilter`.

**Methods:**

- `InvokeAsync` — Rejects invalid guild route values or stores the parsed identifier before calling the next filter.

```csharp
public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await guildIdValidationFilter.InvokeAsync(context, next);
```

---

### MusicEnqueueValidationFilter

**File:** `API/Validation/MusicEnqueueValidationFilter.cs`

**Purpose:** Checks enqueue query shape, provider URLs, and deployed provider enqueue permissions.

**Implements / inherits:** `IEndpointFilter`.

**Methods:**

- `InvokeAsync` — Rejects malformed music inputs and disabled providers before invoking the enqueue handler.

```csharp
public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await musicEnqueueValidationFilter.InvokeAsync(context, next);
```

---

### RequestValidation

**File:** `API/Validation/RequestValidation.cs`

**Purpose:** Provides the legacy HTTP validation result for Discord guild identifiers.

**Methods:**

- `ValidateGuildId` — Returns an HTTP success or bad-request result for the supplied guild identifier.

```csharp
public static IResult ValidateGuildId(string guildId, string action);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = RequestValidation.ValidateGuildId(guildId, action);
```

---

## Related Components

- `API/README.md` — Parent feature overview and neighboring components.
- `API/Errors/` — Defines error categories understood by HTTP result mapping.
- `API/Requests/Queue/` — Defines client-supplied input for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.
- `API/Responses/MusicSearch/` — Defines client-facing output for provider capabilities, search results, and safe provider error responses.
- `API/Services/MusicSearch/` — Validates provider queries and returns bounded track or playlist search results from Lavalink.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
