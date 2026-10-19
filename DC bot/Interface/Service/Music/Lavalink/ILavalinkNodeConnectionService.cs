namespace DC_bot.Interface.Service.Music.Lavalink;

public interface ILavalinkNodeConnectionService
{
    Task ConnectAsync(CancellationToken cancellationToken = default);
}
