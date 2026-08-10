# Service Interfaces

This folder contains contracts consumed by service-layer code.

## Why This Folder Exists

The service layer should coordinate business behavior without knowing the concrete infrastructure behind it. A music service should not know whether queue items come from PostgreSQL, a file, memory, or a test double. It only needs the contract that describes the operations it can call.

This folder is the boundary for those service dependencies.

## Current Subfolders

### Persistence

Repository contracts and persistence records used by bot/API services to read and write persisted state.

## Design Rule

Types here should describe what the service layer needs, not how the infrastructure performs it.

That means:

- method names should express use cases
- parameters should use domain-friendly types such as `ulong` Discord IDs
- return types should be immutable records
- EF Core details should stay out

## When To Add Something Here

Add a new service interface here only when multiple projects or layers need to share the same abstraction. If a type is only used inside one implementation project, keep it local to that project.
