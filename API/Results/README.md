# Results

Represents either a successful value or an expected error before conversion to HTTP.

## Overview

- ApiResult<T> is consumed by DomainToHttpMapper. OAuth redirects and accepted bot commands can return IResult directly when their HTTP semantics differ.
- Use structured failures for expected outcomes. This wrapper does not replace cancellation propagation or unexpected exception handling.

## Models and Results

### ApiResult

**File:** `API/Results/ApiResult.cs`

**Purpose:** Carries either a typed API success value or an error code and message for HTTP mapping.

**Record declaration:**

```csharp
public sealed record ApiResult<T>(bool Success, T? Value, ApiErrorCode? ErrorCode, string? ErrorMessage);
```

**Methods:**

- `Fail` — Creates a failed API result containing an error code and message.

```csharp
public static ApiResult<T> Fail(ApiErrorCode code, string message);
```

- `Ok` — Creates a successful API result containing the supplied value.

```csharp
public static ApiResult<T> Ok(T value);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = ApiResult.Fail(code, message);
```

---

## Related Components

- `API/README.md` — Parent feature overview and neighboring components.
- `API/Errors/` — Defines error categories understood by HTTP result mapping.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
