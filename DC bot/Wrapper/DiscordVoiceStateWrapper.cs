using DC_bot.Interface;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper;

public class DiscordVoiceStateWrapper(DiscordVoiceState discordVoiceState) : IDiscordVoiceState
{
    public IDiscordChannel Channel => new DiscordChannelWrapper(discordVoiceState.Channel);

    public DiscordVoiceState ToDiscordVoiceState()
    {
        return discordVoiceState;
    }
}