using DC_bot.Interface.Discord;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper.Discord;

/// <summary>
/// Adapts discord user data and operations to the bot&apos;s Discord boundary.
/// </summary>
/// <param name="discordUser">The discord user supplied for this operation.</param>
public class DiscordUserWrapper(DiscordUser discordUser) : IDiscordUser
{

    /// <inheritdoc />
    public ulong Id => discordUser.Id;

    /// <inheritdoc />
    public bool IsBot => discordUser.IsBot;

    /// <inheritdoc />
    public string Username => discordUser.Username;

    /// <inheritdoc />
    public string Mention => discordUser.Mention;
}
