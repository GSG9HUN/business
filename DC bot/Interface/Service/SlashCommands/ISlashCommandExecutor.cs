namespace DC_bot.Interface.Service.SlashCommands;

/// <summary>
/// Defines the slash command executor boundary so callers are independent of its concrete implementation.
/// </summary>
public interface ISlashCommandExecutor
{

    /// <summary>
    /// Executes the command request through the registered command pipeline and completes its response.
    /// </summary>
    /// <param name="request">The request supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task ExecuteAsync(SlashCommandExecutionRequest request, CancellationToken cancellationToken = default);
}
