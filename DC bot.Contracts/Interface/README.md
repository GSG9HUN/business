# Interface

This folder groups contracts that are expressed as interfaces.

## Why This Folder Exists

An interface in this project is a promise made by one project and consumed by another. The consuming code should be able to depend on the promise without knowing which concrete class fulfills it.

For example, bot/API code can depend on `IQueueRepository`, while `DC bot.Persistence` provides `QueueRepository`. That lets tests replace the repository with a fake and lets the implementation change without rewriting the service layer.

## Current Shape

Right now this folder contains service-facing interfaces under `Service/`.

The extra `Service` level exists because interfaces can belong to different architectural boundaries. If this project later grows, it could contain API-facing, worker-facing, or integration-facing contracts without mixing them into one flat folder.

## What Belongs Here

- cross-project interfaces
- abstractions used by services
- interfaces whose implementation lives in another project

## What Does Not Belong Here

- implementation classes
- EF Core entities
- request/response DTOs
- test-only fakes

## Maintenance Notes

If all contracts stay persistence-only, a flatter `Persistence/` root would be simpler. If the project grows beyond persistence, keeping `Interface/Service/...` makes the boundary more explicit.
