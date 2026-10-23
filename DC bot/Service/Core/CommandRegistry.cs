using DC_bot.Interface;
using DC_bot.Interface.Core;

namespace DC_bot.Service.Core;

/// <summary>
/// Builds a lazy, name-indexed snapshot of registered commands for discovery and dispatch.
/// </summary>
public sealed class CommandRegistry : ICommandRegistry
{
    private readonly Lazy<CommandRegistrySnapshot> _snapshot;

    /// <summary>
    /// Initializes CommandRegistry with the supplied dependencies and settings.
    /// </summary>
    /// <param name="commandFactory">The deferred collection evaluated when the registry snapshot is first requested.</param>
    public CommandRegistry(Func<IEnumerable<ICommand>> commandFactory)
    {
        _snapshot = new Lazy<CommandRegistrySnapshot>(() => BuildSnapshot(commandFactory), LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <inheritdoc />
    public IReadOnlyCollection<ICommand> Commands => _snapshot.Value.Commands;

    /// <summary>
    /// Looks up a command by name without rebuilding the lazy registry snapshot.
    /// </summary>
    /// <param name="commandName">The command name supplied for this operation.</param>
    /// <param name="command">The command dependency used by this boundary.</param>
    /// <returns>True when a command was found; the out parameter contains that command.</returns>
    public bool TryGetCommand(string commandName, out ICommand command)
    {
        return _snapshot.Value.Lookup.TryGetValue(commandName, out command!);
    }

    /// <summary>
    /// Materializes registered commands and builds their name lookup, rejecting duplicate names during dictionary construction.
    /// </summary>
    /// <param name="commandFactory">The deferred collection evaluated when the registry snapshot is first requested.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    private static CommandRegistrySnapshot BuildSnapshot(Func<IEnumerable<ICommand>> commandFactory)
    {
        var commands = commandFactory()
            .ToList();
        var lookup = commands.ToDictionary(command => command.Name, command => command);
        return new CommandRegistrySnapshot(commands, lookup);
    }
}
