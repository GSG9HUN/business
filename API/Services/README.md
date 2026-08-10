# Services

This folder contains API-local services grouped by feature.

## Why This Folder Exists

The API has workflows that are not simple route mapping and are not persistence implementation. Those workflows live here.

Examples include Discord OAuth calls, app token generation, OAuth state validation, and short-lived auth ticket storage. They are part of the API process, but they should not be placed directly in endpoint declarations or EF repositories.

## Feature Folders

- `Auth/` - Discord OAuth helpers, app token generation, short-lived state and ticket stores

## What Belongs Here

- API-owned infrastructure helpers
- services that use `HttpClient`, `IConfiguration`, `IMemoryCache`, or token APIs
- orchestration helpers that support handlers but are not endpoint declarations

## What Does Not Belong Here

- EF Core repository implementations
- bot runtime services
- request/response DTOs
- route mapping

## Maintenance Notes

- Services may depend on framework abstractions such as `HttpClient`, `IConfiguration`, and `IMemoryCache`.
- Persistence should still go through repository contracts from `DC bot.Contracts`.
