namespace DC_bot.Interface.Service.SlashCommands;

public interface ISlashCommandExecutor
{
    Task ExecuteAsync(SlashCommandExecutionRequest request);
}
