# API

This project contains the HTTP API used by the mobile application and external clients to control the Discord bot.

## Overview

The API is an ASP.NET Core minimal API application. `Program.cs` wires up persistence, authentication, authorization, Discord OAuth services, and endpoint groups under `/api`.

The project should stay focused on HTTP concerns:

- expose routes
- validate incoming route/request data
- translate domain/service results to HTTP responses
- coordinate handlers and persistence contracts

Bot behavior, persistence implementation, and shared contracts live in the other projects.

## Folders

### Endpoints

Contains minimal API route registration extensions.
Endpoint files define URL groups, authorization requirements, filters, and handler bindings.

### Errors

Contains API error codes and shared error response contracts.
Use this folder for HTTP-facing error shapes, not domain logic.

### Handlers

Contains endpoint handler implementations grouped by feature.
Handlers orchestrate request processing and call services/repositories.

### Mapping

Contains mapping helpers that translate domain/service results into HTTP responses.

### Requests

Contains request DTOs grouped by feature.
These records represent client input payloads.

### Responses

Contains response DTOs grouped by feature.
These records represent stable HTTP output payloads.

### Results

Contains API result wrapper types used between handlers, services, and HTTP mapping.

### Services

Contains API-owned infrastructure services, currently focused on authentication/session support.

### Validation

Contains endpoint filters and helper validation used by minimal API routes.

## Root Files

### Program.cs

Application entry point.
Registers services, configures JWT authentication, enables authorization, and maps endpoint groups.
Loads the repository root `.env` file before building the application configuration.

### API.csproj

Project file for package references and build configuration.

### API.http

Local HTTP scratch file for manually calling API endpoints during development.

### appsettings.json

Base API configuration.

### appsettings.Development.json

Development-only API configuration.

### Dockerfile

Container build definition for the API service.

## Environment Configuration

The API reads runtime configuration from environment variables. For local development, `Program.cs` loads the repository root `.env` file before creating the web application builder.

Required keys:

- `POSTGRES_HOST`
- `POSTGRES_PORT`
- `POSTGRES_DB`
- `POSTGRES_USER`
- `POSTGRES_PASSWORD`
- `DISCORD_OAUTH_CLIENT_ID`
- `DISCORD_OAUTH_CLIENT_SECRET`
- `DISCORD_OAUTH_REDIRECT_URI`
- `DISCORD_OAUTH_ANDROID_REDIRECT_URI`
- `APP_AUTH_SIGNING_KEY`
- `APP_AUTH_ISSUER`
- `APP_AUTH_AUDIENCE`

The uppercase `.env` keys are mapped into the internal .NET configuration keys used by the auth services, such as `DiscordOAuth:ClientId` and `AppAuth:SigningKey`.

## Notes

- Keep endpoint classes thin; route behavior belongs in handlers.
- Keep request/response DTOs feature-scoped under their matching subfolders.
- Keep persistence access behind contracts from the contracts/persistence projects.
- Do not put Discord bot command execution logic directly into this project.
