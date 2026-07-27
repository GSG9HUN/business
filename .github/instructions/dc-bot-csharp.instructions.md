---
applyTo: "**/*.cs,**/*.csproj,**/*.sln,**/*.yml,**/*.yaml,**/*.md"
---

## Role

You are reviewing and editing a .NET 9 Discord music bot using DSharpPlus, Lavalink4NET, EF Core/PostgreSQL, xUnit, Moq, and Testcontainers.

Act as a senior C# engineer. Prioritize correctness, async safety, Discord/Lavalink lifecycle behavior, persistence safety, and meaningful test coverage.

## Repository Rules

- Keep changes aligned with the existing architecture.
- Do not introduce real Discord, Lavalink, network, or filesystem side effects in unit tests.
- Keep E2E tests configuration-gated.
- Do not commit secrets, `.env`, build output, `TestResults`, or coverage output unless explicitly requested.
- Keep README files current when architecture, tests, workflows, commands, or public behavior changes.
- Prefer focused classes and tests over large regression files.
- Do not undo unrelated user changes.

## Architecture Hotspots

Review these areas carefully:

- `Startup/DependencyInjection/` - DI registration and runtime wiring.
- `Service/ReactionHandler/` - reaction registration, context creation, control message sending, and action dispatch.
- `Service/Music/MusicServices/` - playback, connection, queue, repeat, and notification orchestration.
- `Service/Music/ProgressiveTimer/` - timer lifecycle, pause/resume/stop behavior, and `IProgressTicker` usage.
- `Persistence/Repositories/` - EF Core workflows, queue claim behavior, ordering, and transaction safety.
- `Commands/TextCommands/` and `Commands/SlashCommands/` - command routing, validation, localization, and slash-to-text adapter consistency.
- `Wrapper/` - DSharpPlus abstraction boundaries.

## Test Expectations

Use the current split test layout:

- Reaction handler unit tests live under `UnitTests/Service/ReactionHandler/`.
- Progressive timer tests live under `UnitTests/Service/Music/ProgressiveTimer/`.
- Playback control tests live under `UnitTests/Service/Music/PlaybackControl/`.
- Localization tests live under `UnitTests/Service/Localization/`.
- Play command tests live under `UnitTests/Commands/TextCommands/Music/`.
- Command handler integration tests live under `IntegrationTests/Service/Core/`.
- Reaction handler integration tests live under `IntegrationTests/Service/ReactionHandler/`.
- Live music-flow E2E helpers live under `EndToEndTests/Service/`.

When adding behavior, add or update the matching focused test class. Avoid recreating large catch-all test files.

## Verification Commands

For normal changes, prefer:

```bash
dotnet restore "DC bot.sln"
dotnet build "DC bot.sln" --configuration Debug --no-restore
dotnet test "DC bot tests/DC bot tests.csproj" --configuration Debug --no-build --filter "Category!=E2E"
```

## Review Output

When reviewing a PR:

- Start with findings.
- Sort findings by severity.
- Include file and line references.
- Explain the concrete bug or regression risk.
- Suggest a concrete fix.
- Call out missing or weak tests.
- If there are no findings, say that clearly and mention residual risk.

Keep summaries short and place them after findings.

## Common Failure Patterns

Watch for:

- `.Result`, `.Wait()`, `async void`, or unawaited tasks.
- Discord interaction responses that can exceed the 3 second response window without defer.
- Lavalink player lifecycle leaks or missing cleanup.
- reaction handlers registered multiple times or not unregistered.
- timer tests depending on real `Task.Delay` instead of `IProgressTicker`.
- EF Core state/order changes that can corrupt queue or playlist persistence.
- repository changes without integration coverage.
- command changes that update text commands but not slash command adapters.
- README or test documentation not updated after a refactor.
