# Missing Or Partial E2E Flows

This file tracks E2E gaps identified from the current test suite.

Note: `UnitTests/Commands/SlashCommands/SlashCommandTestBase.cs` is a unit-test helper. The items below are E2E coverage gaps, so they are documented under `EndToEndTests`.

The previous slash command E2E-category pipeline tests were removed because they did not invoke real Discord slash interactions.

## Missing Or Partial Coverage

1. Real Discord slash invocation E2E is not automated.
2. Most text commands are not covered through real `MessageCreated` E2E routing.
3. Live music E2E calls `ICommand.ExecuteAsync` directly, so it does not cover the full prefix parsing, command lookup, and message wrapper route.
4. Text playlist E2E does not cover `!savePlaylist`.
5. Text playlist E2E does not cover `!addSong`.
6. `!play` E2E covers only the query path, not the URL/source path.
7. `!language` text command has no E2E coverage.
8. `/language` E2E covers `hu`, but not `eng`.
9. Queue text flows do not have dedicated E2E coverage for `!viewList`, `!shuffle`, and `!clear`.
10. Live playback reaction flow does not cover the `:repeat:` reaction.
11. Live playback reaction flow does not cover reaction-removed events.
12. Wrapper E2E does not have dedicated live coverage for `DiscordUserWrapper`, `LavalinkTrackWrapper`, `SlashInteractionContextWrapper`, or `SlashInteractionMessageWrapper`.
