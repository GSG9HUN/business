using System.ComponentModel;
using DC_bot.Constants;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace DC_bot.Commands.SlashCommands.Queue;

/// <summary>
/// Adapts the clear slash command to shared command execution and interaction acknowledgement.
/// </summary>
/// <param name="slashCommandExecutor">The slash command executor dependency used by this boundary.</param>
/// <param name="contextFactory">The slash interaction context factory dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
public class ClearSlashCommand(
    ISlashCommandExecutor slashCommandExecutor,
    ISlashInteractionContextFactory contextFactory,
    ILocalizationService localizationService)
{
    private const string CommandName = "clear";

    /// <summary>
    /// Adapts the clear slash route to its shared handler and interaction response policy.
    /// </summary>
    /// <param name="context">The context supplied for this operation.</param>
    /// <param name="confirm">The confirm supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    [Command("clear")]
    [Description("Clear the queue")]
    public Task Clear(
        SlashCommandContext context,
        [Parameter("confirm")][Description("Must be true to clear the queue")] bool confirm = false)
    {
        return ExecuteAsync(contextFactory.Create(context), confirm);
    }

    /// <summary>
    /// Routes the slash interaction through the shared command executor and acknowledgement policy.
    /// </summary>
    /// <param name="context">The slash interaction context dependency used by this boundary.</param>
    /// <param name="confirm">The confirm supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task ExecuteAsync(ISlashInteractionContext context, bool confirm)
    {
        if (!confirm)
        {
            await context.RespondAsync(GetLocalizedMessage(context, LocalizationKeys.ClearCommandConfirmationRequired));
            return;
        }

        await slashCommandExecutor.ExecuteAsync(new SlashCommandExecutionRequest(CommandName, context, RequireGuild: true, Defer: true), default);
    }

    /// <summary>
    /// Resolves a response template and applies formatting arguments.
    /// </summary>
    /// <param name="context">The slash interaction context dependency used by this boundary.</param>
    /// <param name="key">The key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    private string GetLocalizedMessage(ISlashInteractionContext context, string key, params object[] args)
    {
        return context.GuildId is { } guildId ? localizationService.Get(guildId, key, args) : localizationService.Get(key, args);
    }
}
