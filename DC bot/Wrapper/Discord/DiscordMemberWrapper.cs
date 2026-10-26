using DC_bot.Interface.Discord;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper.Discord;

/// <summary>
/// Adapts discord member data and operations to the bot&apos;s Discord boundary.
/// </summary>
/// <param name="discordMember">The discord member supplied for this operation.</param>
/// <param name="voiceState">The voice state supplied for this operation.</param>
public class DiscordMemberWrapper(DiscordMember discordMember, DiscordVoiceState? voiceState = null) : IDiscordMember
{

    /// <inheritdoc />
    public ulong Id => discordMember.Id;

    /// <inheritdoc />
    public bool IsBot => discordMember.IsBot;

    /// <inheritdoc />
    public string Username => discordMember.Username;

    /// <inheritdoc />
    public string Mention => discordMember.Mention;

    /// <inheritdoc />
    public IDiscordVoiceState VoiceState => new DiscordVoiceStateWrapper(voiceState ?? discordMember.VoiceState);

    /// <summary>
    /// Returns the underlying DSharpPlus member represented by the adapter.
    /// </summary>
    /// <returns>The value produced for the supplied operation context.</returns>
    public DiscordMember ToDiscordMember()
    {
        return discordMember;
    }
}
