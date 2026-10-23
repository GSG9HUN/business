using DC_bot.Interface.Discord;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper.Discord;

/// <summary>
/// Adapts discord voice state data and operations to the bot&apos;s Discord boundary.
/// </summary>
/// <param name="discordVoiceState">The discord voice state supplied for this operation.</param>
public class DiscordVoiceStateWrapper(DiscordVoiceState? discordVoiceState) : IDiscordVoiceState
{

    /// <inheritdoc />
    public IDiscordChannel? Channel
    {
        get
        {
            if (discordVoiceState?.ChannelId is null or 0)
            {
                return null;
            }

            var channel = discordVoiceState.GetChannelAsync()
                .GetAwaiter()
                .GetResult();
            return channel is null ? null : new DiscordChannelWrapper(channel);
        }
    }

    /// <summary>
    /// Returns the underlying DSharpPlus voice state represented by the adapter.
    /// </summary>
    /// <returns>The value produced for the supplied operation context.</returns>
    public DiscordVoiceState ToDiscordVoiceState()
    {
        return discordVoiceState ?? throw new InvalidOperationException("DiscordVoiceState is null in DiscordVoiceStateWrapper.");
    }
}
