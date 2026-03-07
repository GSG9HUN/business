using DC_bot.Helper;
using DC_bot.Interface;
using DC_bot.Interface.Service.Music;
using DC_bot.Logging;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.SlashCommands
{
    public abstract class PlaySlashCommand : ApplicationCommandModule
    {
        private const string CommandNamePlay = "play";
        
        // Property injection supported by DSharpPlus SlashCommands
        public ILavaLinkService LavaLinkService { private get; set; } = null!;
        public ILogger<PlaySlashCommand> Logger { private get; set; } = null!;

        [SlashCommand("play", "Start playing music in the voice channel")]
        public async Task Play(
            InteractionContext ctx,
            [Option("query", "URL or search query")]
            string query)
        {
            Logger.CommandInvoked(CommandNamePlay);

           /* if (!await SlashCommandResponseHelper.DeferAndRequireGuildAsync(ctx, "This command can only be used in a server."))
            {
                return;
            }

            var voiceChannel = await SlashCommandResponseHelper.TryGetVoiceChannelAfterDeferAsync(
                ctx,
                "You must be in a voice channel to play music.");
            if (voiceChannel == null)
            {
                Logger.ValidationUserNotInVoiceChannel();
                return;
            }

            var textChannel = ctx.Channel;

            if (Uri.TryCreate(query, UriKind.Absolute, out var url))
            {
                // TODO: A PlayAsyncUrl és PlayAsyncQuery metódushívások ki vannak kommentelve, ezért a slash parancs
                //       semmit nem csinál (csak "Now playing your request!" üzenetet küld vissza). A tényleges
                //       zenelejátszás nincs megvalósítva ebben a parancsban.
                // await LavaLinkService.PlayAsyncUrl(member.VoiceState.Channel, url, textChannel);
            }
            else
            {
                //await LavaLinkService.PlayAsyncQuery(member.VoiceState.Channel, query, textChannel);
            }

            await SlashCommandResponseHelper.RespondAfterDeferAsync(ctx, "Now playing your request!");
            Logger.CommandExecuted(CommandNamePlay);*/
        }
    }
}