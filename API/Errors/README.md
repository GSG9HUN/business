# Errors

Defines error categories understood by HTTP result mapping.

## Overview

- Handlers create expected failures with ApiErrorCode; DomainToHttpMapper selects their HTTP status and body.
- Keep domain-facing failure categories separate from provider-specific search errors and exception stack traces.

## Models and Results

### ApiErrorCode

**File:** `API/Errors/ApiErrorCode.cs`

**Purpose:** Names the expected API failures mapped by DomainToHttpMapper to HTTP status codes and response bodies.

**Values:**

```csharp
public enum ApiErrorCode
{
    // The failure does not have a more specific API error classification.
    Unknown = 0,
    // The requested resource was not found.
    NotFound = 1,
    // The request conflicts with the current resource state.
    Conflict = 2, // e.g., playlist name conflict
    // The request failed validation.
    Validation = 3, // e.g., invalid name
    // The bot is not connected to the required voice channel.
    NotConnected = 4, // e.g., Lavalink not connected
    // No active playback player is available.
    NoActivePlayer = 5, // e.g., no active player
    // The caller is not permitted to perform the operation.
    Forbidden = 6, // e.g., forbidden action
    // The database is unavailable.
    DbUnavailable = 7, // e.g., database unavailable
    // The supplied input is invalid.
    InvalidInput = 8, // e.g., invalid input provided
    // The request does not provide valid authentication.
    Unauthorized = 9,
}
```

---

## Related Components

- `API/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
