using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using System.ComponentModel;

namespace DC_bot.Commands.SlashCommands.Music;

public class PlaySlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory)
{
    private const string CommandName = "play";

    [Command("play")]
    [Description("Start playing music in the voice channel")]
    public Task Play(
        SlashCommandContext context,
        [Parameter("query")]
        [Description("URL or search query")]
        string query)
    {
        return ExecuteAsync(contextFactory.Create(context), query);
    }

    public Task ExecuteAsync(ISlashInteractionContext context, string query)
    {
        return slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(
            CommandName,
            context,
            query,
            RequireGuild: true,
            Defer: true,
            EnsureDeferredResponse: true));
    }
}
