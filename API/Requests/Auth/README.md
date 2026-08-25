# Auth Requests

This folder contains request DTOs for mobile authentication endpoints.

## Why This Folder Exists

The mobile auth flow has client-provided values that should be treated as HTTP input, not persistence records. A one-time ticket and a refresh token are transport values coming from Android, and handlers decide how to validate and use them.

## Files

### ExchangeRequest.cs

Request body for `/api/auth/exchange`.

Fields:

- `Ticket` - one-time ticket issued by the Discord callback redirect flow

### RefreshRequest.cs

Request body for `/api/auth/refresh` and `/api/auth/logout`.

Fields:

- `RefreshToken` - opaque mobile app refresh token

## Flow Context

`ExchangeRequest` is used after Discord callback redirect gives Android a one-time ticket. `RefreshRequest` is used when Android rotates or revokes its app session.

## Maintenance Notes

- Refresh tokens are sent by the client but stored server-side only as hashes.
- Tickets should be short-lived and single-use.
