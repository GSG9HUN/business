using DC_bot.Logging;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.SlashCommands;

public abstract class PingSlashCommand : ApplicationCommandModule
{
    private const string CommandNamePing = "ping";

    // Property injection supported by DSharpPlus SlashCommands
    public ILogger<PingSlashCommand> Logger { private get; set; } = null!;

    [SlashCommand("ping", "Replies with Pong!")]
    public Task Ping(InteractionContext ctx)
    {
        Logger.CommandInvoked(CommandNamePing);
        //await SlashCommandResponseHelper.RespondAsync(ctx, "Pong!");
        Logger.CommandExecuted(CommandNamePing);
        return Task.CompletedTask;
    }
}
