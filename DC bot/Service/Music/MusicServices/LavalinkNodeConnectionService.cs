using DC_bot.Exceptions.Music;
using DC_bot.Interface.Service.Music;
using DC_bot.Logging;
using Lavalink4NET;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.MusicServices;

public class LavalinkNodeConnectionService(
    IAudioService audioService,
    ILogger<LavalinkNodeConnectionService> logger) : ILavalinkNodeConnectionService
{
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private bool _isAudioServiceStarted;

    public async Task ConnectAsync()
    {
        await _connectLock.WaitAsync().ConfigureAwait(false);

        try
        {
            if (!_isAudioServiceStarted)
            {
                await audioService.StartAsync().ConfigureAwait(false);
                _isAudioServiceStarted = true;
            }

            // Starting the service and having a ready node are separate states.
            // Recheck readiness after reconnects without starting the service again.
            await audioService.WaitForReadyAsync().ConfigureAwait(false);
            logger.LavalinkNodeConnectedSuccessfully();
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
