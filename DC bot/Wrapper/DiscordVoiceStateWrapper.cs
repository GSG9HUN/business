using DC_bot.Interface.Discord;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper;

public class DiscordVoiceStateWrapper(DiscordVoiceState? discordVoiceState) : IDiscordVoiceState
{
    public IDiscordChannel? Channel
    {
        get
        {
            if (discordVoiceState?.ChannelId is null or 0) return null;

            var channel = discordVoiceState.GetChannelAsync().GetAwaiter().GetResult();
            return channel is null ? null : new DiscordChannelWrapper(channel);
        }
    }

    public DiscordVoiceState ToDiscordVoiceState()
    {
        return discordVoiceState
               ?? throw new InvalidOperationException("DiscordVoiceState is null in DiscordVoiceStateWrapper.");
    }
}
