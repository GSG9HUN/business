using DC_bot.Exceptions.Music;
using DC_bot.Service.Music.MusicServices;
using Lavalink4NET;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music;

[Trait("Category", "Unit")]
public class LavalinkNodeConnectionServiceTests
{
    private readonly Mock<IAudioService> _audioServiceMock = new();
    private readonly Mock<ILogger<LavalinkNodeConnectionService>> _loggerMock = new();
    private readonly LavalinkNodeConnectionService _service;

    public LavalinkNodeConnectionServiceTests()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _service = new LavalinkNodeConnectionService(_audioServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ConnectAsync_WhenCalledTwice_StartsAudioOnlyOnce()
    {
        _audioServiceMock.Setup(a => a.StartAsync(CancellationToken.None)).Returns(new ValueTask());

        await _service.ConnectAsync();
        await _service.ConnectAsync();

        _audioServiceMock.Verify(a => a.StartAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ConnectAsync_WhenAudioStartFails_ThrowsLavalinkOperationException()
    {
        _audioServiceMock.Setup(a => a.StartAsync(CancellationToken.None))
            .ThrowsAsync(new InvalidOperationException("boom"));

        await Assert.ThrowsAsync<LavalinkOperationException>(() => _service.ConnectAsync());
    }

    [Fact]
    public async Task ConnectAsync_WhenCalledConcurrently_StartsAudioOnlyOnce()
    {
        _audioServiceMock.Setup(a => a.StartAsync(CancellationToken.None)).Returns(new ValueTask());

        await Task.WhenAll(_service.ConnectAsync(), _service.ConnectAsync());

        _audioServiceMock.Verify(a => a.StartAsync(CancellationToken.None), Times.Once);
    }
}
