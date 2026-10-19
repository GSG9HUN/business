using DC_bot.Exceptions.Music;
using DC_bot.Interface.Service.Music.Lavalink;
using DC_bot.Logging;
using Lavalink4NET;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.Lavalink;

public class LavalinkNodeConnectionService(
    IAudioService audioService,
    ILogger<LavalinkNodeConnectionService> logger) : ILavalinkNodeConnectionService
{
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private bool _isAudioServiceStarted;

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        await _connectLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (!_isAudioServiceStarted)
            {
                await audioService.StartAsync(cancellationToken).ConfigureAwait(false);
                _isAudioServiceStarted = true;
            }

            // Starting the service and having a ready node are separate states.
            // Recheck readiness after reconnects without starting the service again.
            await audioService.WaitForReadyAsync(cancellationToken).ConfigureAwait(false);
            logger.LavalinkNodeConnectedSuccessfully();
        }
        catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LavalinkConnectionFailed(ex, ex.Message);
            throw new LavalinkOperationException("ConnectAsync", "Failed to connect to Lavalink node", ex);
        }
        finally
        {
            _connectLock.Release();
        }
    }
}
