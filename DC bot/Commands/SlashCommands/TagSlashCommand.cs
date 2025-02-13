using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DC_bot.Commands.SlashCommands
{
    public abstract class TagSlashCommand : ApplicationCommandModule
    {
        [SlashCommand("tag", "You can tag someone")]
        public async Task Tag(InteractionContext ctx,
            [Option("name", "Name of the member you want to tag.")]
            string username)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            if (ctx.Guild == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent("This command can only be used in a server."));
                return;
            }

            var allMembers = await ctx.Guild.GetAllMembersAsync();
            var member =
                allMembers.FirstOrDefault(m =>
                    m.Username.Contains(username, StringComparison.OrdinalIgnoreCase));
            if (member == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent($"User '{username}' not found."));
                return;
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"You tagged: {member.Mention}"));
        }
    }
}