using DC_bot.Exceptions.Music;
using DC_bot.Interface.Service.Music.Lavalink;
using DC_bot.Logging;
using Lavalink4NET;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.Lavalink;

/// <summary>
/// Connects the configured Lavalink node and records connection failures.
/// </summary>
/// <param name="audioService">The audio service dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
public class LavalinkNodeConnectionService(
    IAudioService audioService,
    ILogger<LavalinkNodeConnectionService> logger) : ILavalinkNodeConnectionService
{
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private bool _isAudioServiceStarted;

    /// <summary>
    /// Connects the configured Discord or Lavalink runtime boundary and propagates connection failures.
    /// </summary>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _connectLock.WaitAsync(cancellationToken)
            .ConfigureAwait(false);
        try
        {
            if (!_isAudioServiceStarted)
            {
                await audioService.StartAsync(cancellationToken)
                    .ConfigureAwait(false);
                _isAudioServiceStarted = true;
            }

            // Starting the service and having a ready node are separate states.
            // Recheck readiness after reconnects without starting the service again.
            await audioService.WaitForReadyAsync(cancellationToken)
                .ConfigureAwait(false);
            logger.LavalinkNodeConnectedSuccessfully();
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
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
