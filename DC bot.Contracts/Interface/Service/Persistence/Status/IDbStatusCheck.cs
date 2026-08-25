namespace DC_bot.Interface.Service.Persistence.Status;

public interface IDbStatusCheck
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
}