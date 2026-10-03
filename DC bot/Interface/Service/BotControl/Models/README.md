# Bot Control Models

This folder contains bot-control execution models shared by bot-control services.

## Files

### BotControlCommandResult.cs

Normalized command result object persisted after command execution. It carries success/failure information and optional result metadata.

### BotControlExecutionContext.cs

Resolved Discord execution context for a claimed command, including guild/channel/member details where available.

## Notes

- Models here describe bot-side execution, not HTTP request/response payloads.
- API response DTOs live under `API/Responses/BotControl/`.
