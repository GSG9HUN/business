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
        if (_isAudioServiceStarted)
        {
            logger.LogDebug("Lavalink node connection requested, but audio service is already started.");
            return;
        }

        await _connectLock.WaitAsync().ConfigureAwait(false);

        try
        {
            if (_isAudioServiceStarted)
            {
                logger.LogDebug("Lavalink node connection skipped because another caller already started the audio service.");
                return;
            }

            await audioService.StartAsync().ConfigureAwait(false);
            await audioService.WaitForReadyAsync().ConfigureAwait(false);
            _isAudioServiceStarted = true;
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
