namespace DC_bot.Interface.Service.BotControl;

public interface IBotControlWorker
{
    Task RunAsync(CancellationToken cancellationToken);
}
