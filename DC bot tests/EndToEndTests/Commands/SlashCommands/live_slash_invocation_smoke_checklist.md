# Live Slash Invocation Smoke Checklist

Use this checklist with a real Discord client in the configured test guild after the bot has started successfully.

Required environment:

- `DISCORD_TOKEN`
- `DISCORD_TEST_GUILD_ID`
- `DISCORD_TEST_CHANNEL_ID`
- `DISCORD_TEST_VOICE_CHANNEL_ID`
- `LAVALINK_HOSTNAME`
- `LAVALINK_PORT`
- `LAVALINK_PASSWORD`

Preconditions:

- The bot is online in the test guild.
- Slash commands are visible in the Discord client command picker.
- The tester account is in `DISCORD_TEST_VOICE_CHANNEL_ID` before running music commands.
- Lavalink is reachable by the bot process.

Smoke steps:

- [ ] `/ping` returns `Pong!`.
- [ ] `/help` lists the available commands.
- [ ] `/tag user:<member>` returns the selected member mention.
- [ ] `/play query:<known-good-query-or-url>` starts playback or queues the track.
- [ ] `/pause` pauses the current playback.
- [ ] `/resume` resumes the current playback.
- [ ] `/skip` stops/skips the current playback.
- [ ] `/queue` displays the current queue state.
- [ ] `/shuffle` shuffles a queue with at least two tracks.
- [ ] `/repeat track` toggles track repeat.
- [ ] `/repeat list` toggles repeat-list mode when a current track and queue exist.
- [ ] `/language language:hu` returns a Hungarian localized response.
- [ ] `/language language:eng` returns an English localized response.
- [ ] `/clear confirm:false` does not clear the queue.
- [ ] `/clear confirm:true` clears the queue.
- [ ] `/leave` disconnects the bot from the voice channel.

Pass criteria:

- Every command appears in Discord's slash command picker with the expected options.
- Every command returns a localized user-facing response or performs the expected playback action.
- No command produces an unlocalized fallback, raw exception, or duplicate Discord response.
- Music commands leave the bot disconnected after `/leave`.
