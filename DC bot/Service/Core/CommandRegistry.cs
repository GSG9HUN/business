using DC_bot.Interface;
using DC_bot.Interface.Core;
using System.Threading;

namespace DC_bot.Service.Core;

public sealed class CommandRegistry : ICommandRegistry
{
    private readonly Lazy<CommandRegistrySnapshot> _snapshot;

    public CommandRegistry(Func<IEnumerable<ICommand>> commandFactory)
    {
        _snapshot = new Lazy<CommandRegistrySnapshot>(
            () => BuildSnapshot(commandFactory),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public IReadOnlyCollection<ICommand> Commands => _snapshot.Value.Commands;

    public bool TryGetCommand(string commandName, out ICommand command)
    {
        return _snapshot.Value.Lookup.TryGetValue(commandName, out command!);
    }

    private static CommandRegistrySnapshot BuildSnapshot(Func<IEnumerable<ICommand>> commandFactory)
    {
        var commands = commandFactory().ToList();
        var lookup = commands.ToDictionary(command => command.Name, command => command);
        return new CommandRegistrySnapshot(commands, lookup);
    }

    private sealed record CommandRegistrySnapshot(
        IReadOnlyList<ICommand> Commands,
        IReadOnlyDictionary<string, ICommand> Lookup);
}
