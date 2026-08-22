# Queue Requests

This folder contains request DTOs for queue operations.

## Why This Folder Exists

Queue write endpoints need client input such as a URL, search query, or requested search mode. This folder keeps that HTTP input shape separate from queue persistence records and bot-control command records.

## Files

### EnqueueRequest.cs

Carries a query or URL and optional search mode for adding media to a guild queue.

Fields:

- `Query` - search query, track URL, or playlist URL
- `SearchMode` - optional client-selected search behavior

## Flow Context

The API receives this request, validates route/user context, and then maps the work into the correct command or service flow. The request object itself should not know how playback, queue persistence, or bot command dispatch works.

## Maintenance Notes

- Queue size and playback rules are enforced by service/repository layers, not by this DTO.
- Keep this DTO focused on client input only.
