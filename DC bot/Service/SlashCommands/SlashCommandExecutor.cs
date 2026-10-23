using DC_bot.Constants;
using DC_bot.Exceptions;
using DC_bot.Interface;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.SlashCommands;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.SlashCommands;

/// <summary>
/// Adapts slash interactions to registered commands and completes deferred interaction responses.
/// </summary>
/// <param name="commands">The commands supplied for this operation.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
public class SlashCommandExecutor(
    IEnumerable<ICommand> commands,
    ILogger<SlashCommandExecutor> logger,
    ILocalizationService localizationService) : ISlashCommandExecutor
{
    private readonly IReadOnlyDictionary<string, ICommand> _commands = commands.ToDictionary(command => command.Name, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Executes the command request through the registered command pipeline and completes its response.
    /// </summary>
    /// <param name="request">The request supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task ExecuteAsync(SlashCommandExecutionRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ExecuteInternalAsync(request.CommandName, request.Context, request.Argument, request.RequireGuild, request.Defer, request.EnsureDeferredResponse, cancellationToken);
    }

    /// <summary>
    /// Acknowledges the slash interaction, adapts its arguments to a command message, and completes the registered command response.
    /// </summary>
    /// <param name="commandName">The command name supplied for this operation.</param>
    /// <param name="context">The slash interaction context dependency used by this boundary.</param>
    /// <param name="argument">The argument supplied for this operation.</param>
    /// <param name="requireGuild">The require guild supplied for this operation.</param>
    /// <param name="defer">The defer supplied for this operation.</param>
    /// <param name="ensureDeferredResponse">The ensure deferred response supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    private async Task ExecuteInternalAsync(
        string commandName,
        ISlashInteractionContext context,
        string? argument = null,
        bool requireGuild = false,
        bool defer = false,
        bool ensureDeferredResponse = false,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.CommandInvoked(commandName);

        if (requireGuild && context.Guild is null)
        {
            await context.RespondAsync(GetLocalizedMessage(context, LocalizationKeys.SlashCommandGuildOnly))
                .WaitAsync(cancellationToken);
            return;
        }

        if (!_commands.TryGetValue(commandName, out var command))
        {
            await context.RespondAsync(GetLocalizedMessage(context, LocalizationKeys.SlashCommandNotRegistered, commandName))
                .WaitAsync(cancellationToken);
            return;
        }

        if (defer)
        {
            await context.DeferAsync()
                .WaitAsync(cancellationToken);
        }

        try
        {
            await command.ExecuteAsync(context.CreateMessage(commandName, argument));

            if (ensureDeferredResponse && context.IsDeferred && !context.HasResponded)
            {
                await context.RespondAsync(GetLocalizedMessage(context, LocalizationKeys.SlashCommandDeferredAccepted))
                    .WaitAsync(cancellationToken);
            }

            logger.CommandExecuted(commandName);
        }
        catch (BotException exception)
        {
            logger.CommandExecutionFailed(exception, commandName);

            if (!context.HasResponded)
            {
                await context.RespondAsync(exception.Message)
                    .WaitAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.CommandExecutionFailed(exception, commandName);

            if (!context.HasResponded)
            {
                await context.RespondAsync(GetLocalizedMessage(context, LocalizationKeys.SlashCommandUnexpectedError))
                    .WaitAsync(cancellationToken);
            }
        }
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
