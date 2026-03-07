using DC_bot.Interface.Discord;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper;

public class DiscordVoiceStateWrapper(DiscordVoiceState? discordVoiceState) : IDiscordVoiceState
{
    public IDiscordChannel? Channel => discordVoiceState?.Channel is null
        ? null
        : new DiscordChannelWrapper(discordVoiceState.Channel);

    public DiscordVoiceState ToDiscordVoiceState()
    {
        return discordVoiceState
               ?? throw new InvalidOperationException("DiscordVoiceState is null in DiscordVoiceStateWrapper.");
    }
}