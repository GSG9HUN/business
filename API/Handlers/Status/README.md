# Status Handlers

This folder contains API health/status handlers.

## Why This Folder Exists

Status endpoints are operational endpoints. They tell deployment tooling, developers, or monitoring whether the API and its required dependencies are reachable.

Keeping status handling separate from feature handlers prevents health checks from being mixed with user-facing API workflows.

## Files

### StatusHandler.cs

Checks database connectivity through `IDbStatusCheck`.

Responsibilities:

- verify whether the configured database is reachable
- map database availability to API results
- avoid exposing implementation-specific exception details to clients

## Flow Context

The handler calls the persistence status contract instead of opening EF Core directly. This keeps the API layer focused on HTTP response shape while the persistence project owns how database connectivity is checked.

## Maintenance Notes

- This handler is safe to keep unauthenticated if it is used for deployment health checks.
- Avoid adding expensive dependency checks unless the status endpoint is explicitly intended to be deep health validation.
