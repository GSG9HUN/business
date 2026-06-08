using System.ComponentModel;
using DC_bot.Constants;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Queue;

public class ClearSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory,
    ILocalizationService localizationService)
{
    private const string CommandName = "clear";

    [Command("clear")]
    [Description("Clear the queue")]
    public Task Clear(
        SlashCommandContext context,
        [Parameter("confirm")]
        [Description("Must be true to clear the queue")]
        bool confirm = false)
    {
        return ExecuteAsync(contextFactory.Create(context), confirm);
    }

    public async Task ExecuteAsync(ISlashInteractionContext context, bool confirm)
    {
        if (!confirm)
        {
            await context.RespondAsync(GetLocalizedMessage(context, LocalizationKeys.ClearCommandConfirmationRequired));
            return;
        }

        await slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(
            CommandName,
            context,
            RequireGuild: true,
            Defer: true));
    }

    private string GetLocalizedMessage(ISlashInteractionContext context, string key, params object[] args)
    {
        return context.GuildId is { } guildId
            ? localizationService.Get(guildId, key, args)
            : localizationService.Get(key, args);
    }
}
