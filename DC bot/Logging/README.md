# Logging extensions

Ez a mappa az egységes naplózási (ILogger) kiterjesztéseket és scope helper‑t tartalmazza.

## Mi van itt?
- `LogExtensions.cs`: `LoggerMessage` alapú, source‑generated logolás.
- `LoggingScopes.cs`: egységes scope helper (pl. parancs végrehajtásnál).

## EventId tábla
Lásd: `DC bot/Logging/EventIdTable.md`

## Használat példa
```csharp
using DC_bot.Logging;

logger.CommandInvoked(commandName);
logger.CommandExecuted(commandName);

using var scope = logger.BeginCommandScope(commandName, userId, channelId, guildId);
```

## Irányelvek
- Debug: invoked/executed jellegű naplók
- Information: állapotváltozás, fontos esemény
- Warning: nem várt, de kezelhető esetek
- Error: kivételek, működési hiba
