using DC_bot.Helper.Factory;
using DC_bot.Wrapper.Discord;
using DSharpPlus.Entities;

namespace DC_bot.Service.ReactionHandler;

/// <summary>
/// Resolves the Discord entities required for a supported music-control reaction.
/// </summary>
public sealed class ReactionContextFactory
{

    /// <summary>
    /// Resolves and constructs the context required by the requested Discord operation.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <param name="user">The user supplied for this operation.</param>
    /// <param name="channel">The channel supplied for this operation.</param>
    /// <param name="guild">The guild supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<ReactionContext> CreateAsync(
        DiscordMessage message,
        DiscordUser user,
        DiscordChannel channel,
        DiscordGuild? guild = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var resolvedGuild = guild ?? (user as DiscordMember)?.Guild ?? channel.Guild;

        if (resolvedGuild is null)
        {
            throw new InvalidOperationException("Reaction event context does not contain a guild.");
        }

        var discordAuthor = new DiscordUserWrapper(user);
        var discordChannel = new DiscordChannelWrapper(channel, guild: resolvedGuild);
        var member = user is DiscordMember discordMember ? new DiscordMemberWrapper(discordMember) : await discordChannel.Guild.GetMemberAsync(discordAuthor.Id)
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);
        var discordMessageWrapper = DiscordMessageWrapperFactory.Create(message, channel, user, guild: resolvedGuild);
        return new ReactionContext(member, discordMessageWrapper, resolvedGuild.Id);
    }
}
