using DC_bot.Services;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.SlashCommands
{
    public class PlaySlashCommand : ApplicationCommandModule
    {
        private readonly LavaLinkService _lavaLinkService = ServiceLocator.GetService<LavaLinkService>();
        private readonly ILogger<PlaySlashCommand> _logger = ServiceLocator.GetService<ILogger<PlaySlashCommand>>();


        [SlashCommand("play", "Start playing music in the voice channel")]
        public async Task Play(
            InteractionContext ctx,
            [Option("query", "URL or search query")]
            string query)
        {
            _logger.LogInformation("Play slash command invoked");

            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (ctx.Guild == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().WithContent(
                        "This command can only be used in a server."));
                return;
            }

            var member = ctx.Member;
            if (member?.VoiceState?.Channel == null)
            {
                _logger.LogInformation("The user is not in a voice channel");
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    "You must be in a voice channel to play music."));
                return;
            }


            var textChannel = ctx.Channel;
            if (Uri.TryCreate(query, UriKind.Absolute, out var url))
            {
                await _lavaLinkService.PlayAsyncUrl(member.VoiceState.Channel, url, textChannel);
            }
            else
            {
                await _lavaLinkService.PlayAsyncQuery(member.VoiceState.Channel, query, textChannel);
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Now playing your request!"));
            _logger.LogInformation("Play slash command executed!");
        }
    }
}