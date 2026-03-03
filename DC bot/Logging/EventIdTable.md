# EventId tábla

Az alábbi táblázat az alkalmazásban használt `LoggerMessage` EventId‑ket és a hozzájuk tartozó üzeneteket sorolja fel.

| EventId | Level | Metódus | Üzenet |
|---:|---|---|---|
| 1001 | Debug | CommandInvoked | Command invoked: {CommandName} |
| 1002 | Debug | CommandExecuted | Command executed: {CommandName} |
| 1003 | Debug | CommandMissingArgument | Command missing argument: {CommandName} |
| 1101 | Information | CommandHandlerAlreadyRegistered | CommandHandler Service is already registered |
| 1102 | Information | CommandHandlerRegistered | Registered command handler |
| 1103 | Error | CommandHandlerNoPrefix | No prefix provided |
| 1104 | Information | CommandHandlerUnknownCommand | Unknown command. Use `!help` to see available commands. |
| 1105 | Information | CommandHandlerUnregistered | Unregistered command handler |
| 1106 | Warning | CommandHandlerUnregisterNotRegistered | Tried to unregister handler, but it was not registered |
| 1201 | Information | ReactionHandlerAlreadyRegistered | ReactionHandler Service is already registered |
| 1202 | Information | ReactionHandlerRegistered | Registered reaction handler. |
| 1203 | Information | ReactionHandlerUnregistered | Unregistered reaction handler |
| 1204 | Warning | ReactionHandlerUnregisterNotRegistered | Tried to unregister handlers, but it was not registered |
| 1205 | Information | ReactionControlMessageSent | Reaction control message sent and reactions added. |
| 1206 | Information | ReactionAdded | Reaction added: {Emoji} by {Username} |
| 1207 | Information | ReactionRemoved | Reaction removed: {Emoji} by {Username} |
| 1301 | Information | LocalizationLoading | Loading localization for {LanguageCode} |
| 1302 | Information | LocalizationLoaded | Localization loaded. |
| 1401 | Information | ValidationLavalinkNotConnected | Lavalink is not connected. |
| 1402 | Information | ValidationBotNotConnected | Bot is not connected to a voice channel. |
| 1403 | Information | ValidationUserIsBot | User is Bot. |
| 1404 | Information | ValidationUserNotInVoiceChannel | User is not in a voice channel. |
| 1501 | Information | DiscordClientLoggerInitialized | Logger initialized for SingletonDiscordClient. |
| 1502 | Information | DiscordClientReady | Bot is ready! |
| 1503 | Information | DiscordClientGuildAvailable | Guild available: {GuildName} |
| 1601 | Information | PlayCommandStartUrl | Starting playing a music through URL. |
| 1602 | Information | PlayCommandStartQuery | Starting playing a music through search result. |
| 1701 | Information | QueueIsEmpty | Queue is empty. Playback has stopped. |
| 2001 | Information | LavalinkNodeConnectedSuccessfully | Lavalink node connected successfully |
| 2002 | Error | LavalinkConnectionFailed | Lavalink connection failed: {Message} |
| 2003 | Information | PlaybackFinishedEventRegistered | PlaybackFinished event registered. |
| 2004 | Information | FailedToFindMusicWithUrl | Failed to find music with url: {Url} |
| 2005 | Information | FailedToFindMusicWithQuery | Failed to find music with query: {Query} |
| 2006 | Information | ThereIsNoTrackCurrentlyPlaying | There is no track currently playing. |
| 2007 | Information | ThereIsNoTrackCurrentlyPaused | There is no track currently paused. |
| 2008 | Information | AddedToQueue | Added to queue. |
| 2009 | Information | AddedToQueueWithDetails | Added to queue: {Author} - {Title} |
| 2011 | Information | NowPlaying | Now Playing: {Author} - {Title} |
| 2012 | Information | Repeating | Repeating: {RepeatTrackAuthor} - {RepeatTrackTitle} |

