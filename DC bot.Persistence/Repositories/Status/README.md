# Status Repositories

This folder contains persistence status repository implementations.

## Why This Folder Exists

Health/status endpoints should be able to verify the configured database path without knowing EF Core details. This repository is the concrete implementation of that small health boundary.

## Files

### DbStatusCheck.cs

Implements `IDbStatusCheck`.

Responsibilities:

- check whether EF Core can connect to the configured database

## Flow Context

The API status handler calls the contract. This implementation creates a short-lived EF context and asks whether the configured database can be reached.

## Maintenance Notes

- This is used by health/status API endpoints.
- Keep this check lightweight; it should not mutate data or run expensive diagnostics.
