namespace DC_bot.Interface.Core;

/// <summary>
/// Defines the command registry boundary so callers are independent of its concrete implementation.
/// </summary>
public interface ICommandRegistry
{

    /// <summary>
    /// The registered text commands available for dispatch.
    /// </summary>
    IReadOnlyCollection<ICommand> Commands { get; }

    /// <summary>
    /// Looks up a command by name without rebuilding the lazy registry snapshot.
    /// </summary>
    /// <param name="commandName">The command name supplied for this operation.</param>
    /// <param name="command">The command dependency used by this boundary.</param>
    /// <returns>True when a command was found; the out parameter contains that command.</returns>
    bool TryGetCommand(string commandName, out ICommand command);
}
