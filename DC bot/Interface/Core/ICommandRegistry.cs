namespace DC_bot.Interface.Core;

public interface ICommandRegistry
{
    IReadOnlyCollection<ICommand> Commands { get; }
    bool TryGetCommand(string commandName, out ICommand command);
}
