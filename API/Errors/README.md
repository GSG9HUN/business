# Errors

This folder contains API-level error codes used by HTTP result mapping.

## Files

### ApiErrorCode.cs

Defines stable error categories returned by handler/service operations.

Current categories include:

- `NotFound`
- `Conflict`
- `Validation`
- `InvalidInput`
- `Unauthorized`
- `Forbidden`
- `DbUnavailable`
- playback-specific availability states

## Purpose

`ApiErrorCode` decouples domain or handler failures from concrete HTTP status codes. The conversion to HTTP responses is handled in `Mapping/DomainToHttpMapper.cs`.

## Notes

- Add a mapper case whenever a new error code is introduced.
- Prefer specific error codes over `Unknown` so mobile clients can handle failures predictably.
