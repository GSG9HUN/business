using DC_bot.Constants;
using DC_bot.Exceptions;
using DC_bot.Interface;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.SlashCommands;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.SlashCommands;

public class SlashCommandExecutor(
    IEnumerable<ICommand> commands,
    ILogger<SlashCommandExecutor> logger,
    ILocalizationService localizationService) : ISlashCommandExecutor
{
    private readonly IReadOnlyDictionary<string, ICommand> _commands =
        commands.ToDictionary(command => command.Name, StringComparer.OrdinalIgnoreCase);

    public Task ExecuteAsync(SlashCommandExecutionRequest request)
    {
        return ExecuteInternalAsync(
            request.CommandName,
            request.Context,
            request.Argument,
            request.RequireGuild,
            request.Defer,
            request.EnsureDeferredResponse);
    }

    private async Task ExecuteInternalAsync(
        string commandName,
        ISlashInteractionContext context,
        string? argument = null,
        bool requireGuild = false,
        bool defer = false,
        bool ensureDeferredResponse = false)
    {
        logger.CommandInvoked(commandName);

        if (requireGuild && context.Guild is null)
        {
            await context.RespondAsync(GetLocalizedMessage(context, LocalizationKeys.SlashCommandGuildOnly));
            return;
        }

        if (!_commands.TryGetValue(commandName, out var command))
        {
            await context.RespondAsync(
                GetLocalizedMessage(context, LocalizationKeys.SlashCommandNotRegistered, commandName));
            return;
        }

        if (defer)
        {
            await context.DeferAsync();
        }

        try
        {
            await command.ExecuteAsync(context.CreateMessage(commandName, argument));

            if (ensureDeferredResponse && context.IsDeferred && !context.HasResponded)
            {
                await context.RespondAsync(
                    GetLocalizedMessage(context, LocalizationKeys.SlashCommandDeferredAccepted));
            }

            logger.CommandExecuted(commandName);
        }
        catch (BotException exception)
        {
            logger.CommandExecutionFailed(exception, commandName);
            if (!context.HasResponded) await context.RespondAsync(exception.Message);
        }
        catch (Exception exception)
        {
            logger.CommandExecutionFailed(exception, commandName);
            if (!context.HasResponded)
                await context.RespondAsync(GetLocalizedMessage(context, LocalizationKeys.SlashCommandUnexpectedError));
        }
    }

    private string GetLocalizedMessage(ISlashInteractionContext context, string key, params object[] args)
    {
        return context.GuildId is { } guildId
            ? localizationService.Get(guildId, key, args)
            : localizationService.Get(key, args);
    }
}
