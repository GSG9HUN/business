namespace DC_bot.Interface.Service.Persistence;

public interface IDbStatusCheck
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
}