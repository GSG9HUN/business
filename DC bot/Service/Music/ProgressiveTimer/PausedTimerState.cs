using DC_bot.Interface.Discord;

namespace DC_bot.Service.Music.ProgressiveTimer;

internal sealed record PausedTimerState(IDiscordMessage Message, string TrackIdentifier, TimeSpan Position);
