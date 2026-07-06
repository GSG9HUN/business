using DC_bot.Helper.Factory;
using DC_bot.Interface.Discord;
using DC_bot.Wrapper;
using DSharpPlus.Entities;

namespace DC_bot.Service.ReactionHandler;

public sealed class ReactionContextFactory
{
    public async Task<ReactionContext> CreateAsync(
        DiscordMessage message,
        DiscordUser user,
        DiscordChannel channel,
        DiscordGuild? guild = null)
    {
        var resolvedGuild = guild ?? (user as DiscordMember)?.Guild ?? channel.Guild;
        if (resolvedGuild is null)
        {
            throw new InvalidOperationException("Reaction event context does not contain a guild.");
        }

        var discordAuthor = new DiscordUserWrapper(user);
        var discordChannel = new DiscordChannelWrapper(channel, guild: resolvedGuild);
        var member = user is DiscordMember discordMember
            ? new DiscordMemberWrapper(discordMember)
            : await discordChannel.Guild.GetMemberAsync(discordAuthor.Id).ConfigureAwait(false);
        var discordMessageWrapper = DiscordMessageWrapperFactory.Create(message, channel, user, guild: resolvedGuild);

        return new ReactionContext(member, discordMessageWrapper, resolvedGuild.Id);
    }
}
