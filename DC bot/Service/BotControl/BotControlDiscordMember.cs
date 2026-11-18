using DC_bot.Interface.Discord;
using DSharpPlus.Entities;

namespace DC_bot.Service.BotControl;

/// <summary>
/// Adapts discord member data and operations to the bot&apos;s Discord boundary.
/// </summary>
/// <param name="discordMember">The discord member supplied for this operation.</param>
/// <param name="responseChannel">The Discord channel used by the operation.</param>
internal sealed class BotControlDiscordMember(DiscordMember discordMember, IDiscordChannel responseChannel) : IDiscordMember
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
    public IDiscordVoiceState? VoiceState { get; } = new BotControlDiscordVoiceState(responseChannel);

    /// <summary>
    /// Returns the underlying DSharpPlus member represented by the adapter.
    /// </summary>
    /// <returns>The value produced for the supplied operation context.</returns>
    public DiscordMember ToDiscordMember()
    {
        return discordMember;
    }
}
