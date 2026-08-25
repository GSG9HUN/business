# Results

This folder contains API result wrappers used before mapping to HTTP responses.

## Why This Folder Exists

Handlers and helper services sometimes need to return either a successful value or a structured failure before the response is converted to HTTP. Result wrappers make that explicit without throwing exceptions for expected outcomes such as validation failures, conflicts, or missing records.

## Files

### ApiResults.cs

Defines `ApiResult<T>`, a simple success/failure wrapper.

Purpose:

- carry successful values
- carry structured error codes and messages
- keep handlers consistent before `DomainToHttpMapper` converts results to HTTP responses

## Flow Context

A handler can build an `ApiResult<T>`, pass it to `DomainToHttpMapper`, and let the mapper choose the final status code and error response shape. That keeps handler code focused on orchestration instead of repeated response construction.

## Maintenance Notes

- Use `ApiResult<T>` for normal JSON API responses.
- Do not force OAuth redirects or accepted command submissions into this wrapper if direct `IResult` values are clearer.
