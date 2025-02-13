using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DC_bot.Commands.SlashCommands
{
    public abstract class PingSlashCommand : ApplicationCommandModule
    {
        [SlashCommand("ping", "Replies with Pong!")]
        public async Task Ping(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Pong!"));
        }
    }
}