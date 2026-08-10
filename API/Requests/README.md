# Requests

This folder contains request DTOs grouped by feature.

## Why This Folder Exists

Request DTOs represent client-provided HTTP request bodies. They are the public input contract of the HTTP API, so they should stay separate from persistence records and EF entities.

This separation matters because client payloads often differ from internal data. A request may contain a one-time auth ticket, a refresh token, a playlist name, or a queue query. None of those should force repository records or database entities to mirror the HTTP shape.

## Feature Folders

- `Auth/` - app session exchange and refresh/logout request bodies
- `Playlists/` - playlist create, rename, save, and track-add request bodies
- `Queue/` - queue enqueue request bodies

## What Belongs Here

- request body DTOs used by minimal API endpoints
- simple client-provided fields
- DTOs grouped by the feature endpoint that receives them

## What Does Not Belong Here

- response DTOs
- EF entities
- persistence records
- validation filters

## Maintenance Notes

- Keep validation rules in endpoint filters, handlers, or dedicated validators rather than embedding database or service calls in DTOs.
- Use feature namespaces such as `API.Requests.Auth` and `API.Requests.Playlists`.
