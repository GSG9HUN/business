namespace DC_bot.Interface.Service.Persistence.BotControl;

public interface IBotControlCommandNotifier
{
    Task WaitForCommandAsync(CancellationToken cancellationToken);
}
