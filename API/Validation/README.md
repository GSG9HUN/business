# Validation

This folder contains validation helpers used by minimal API endpoints.

## Overview

Validation classes should reject malformed route values or request input before handlers run.
They are part of the HTTP layer, so they should return API-friendly errors and keep parsed values in the request context when handlers need them.

## Files

### CommandNameValidationFilter.cs

Endpoint filter for playback command routes.

Responsibilities:

- read `commandName` from route values
- normalize the command name
- allow only supported playback commands
- store the normalized command in `HttpContext.Items["commandName"]`

### GuildIdValidationFilter.cs

Endpoint filter for guild-scoped routes.

Responsibilities:

- read `guildId` from route values
- validate that it is a non-zero Discord snowflake value
- store the parsed guild id in `HttpContext.Items["guildId"]`

### RequestValidation.cs

Legacy/static validation helper for simple request validation.

Prefer endpoint filters for new minimal API route validation when the parsed value is needed by the handler.

## Notes

- Keep validation focused on HTTP input shape and simple parsing.
- Authorization checks should stay in handlers/services, not in validation filters.
- Reuse `ApiErrorCode.Validation` for structured validation errors.
