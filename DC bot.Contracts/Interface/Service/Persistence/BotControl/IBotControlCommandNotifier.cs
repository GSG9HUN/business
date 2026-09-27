namespace DC_bot.Interface.Service.Persistence.BotControl;

public interface IBotControlCommandNotifier
{
    Task EnsureListeningAsync(CancellationToken cancellationToken);
    Task WaitForCommandAsync(CancellationToken cancellationToken);
}
