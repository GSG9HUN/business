using DC_bot.Helper;
using DSharpPlus.SlashCommands;

namespace DC_bot.Commands.SlashCommands;

public abstract class PingSlashCommand : ApplicationCommandModule
{
    [SlashCommand("ping", "Replies with Pong!")]
    public async Task Ping(InteractionContext ctx)
    {
        //await SlashCommandResponseHelper.RespondAsync(ctx, "Pong!");
    }
}