# Mapping

This folder contains API result mapping helpers.

## Why This Folder Exists

Handlers often receive an application-level result that says what happened, but not which HTTP status code should be returned. Mapping belongs here so handlers do not repeat status-code decisions.

The goal is consistency. The same `ApiErrorCode.Unauthorized` should always become the same HTTP status and response shape, regardless of which handler produced it.

## Files

### DomainToHttpMapper.cs

Converts `ApiResult<T>` values into ASP.NET Core `IResult` responses.

Responsibilities:

- convert successful results to `200 OK`
- map error codes to appropriate HTTP status codes
- keep HTTP status mapping consistent across handlers

## What Belongs Here

- conversion from API result codes to HTTP responses
- shared error response mapping
- status-code decisions that should be consistent across handlers

## What Does Not Belong Here

- business logic
- repository queries
- DTO-to-entity persistence mapping

## Maintenance Notes

- Add a mapper branch whenever a new `ApiErrorCode` is introduced.
- Redirects, file responses, streaming responses, and `202 Accepted` command submissions may be returned directly by handlers when they do not fit the `ApiResult<T>` pattern.
