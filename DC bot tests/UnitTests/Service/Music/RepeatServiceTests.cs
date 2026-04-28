using DC_bot.Interface;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Service.Music.MusicServices;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music;

public class RepeatServiceTests
{
    [Fact]
    public async Task Init_DefaultsToFalse()
    {
        var service = new RepeatService(
            new InMemoryPlaybackStateRepository(), 
            new InMemoryRepeatListRepository(),
            new Mock<ILogger<RepeatService>>().Object
            );
        const ulong guildId = 10;

        await service.InitAsync(guildId);

        Assert.False(await service.IsRepeatingAsync(guildId));
        Assert.False(await service.IsRepeatingListAsync(guildId));
    }

    [Fact]
    public async Task SetRepeating_AfterInit_UpdatesFlag()
    {
        var service = new RepeatService(
            new InMemoryPlaybackStateRepository(), 
            new InMemoryRepeatListRepository(),
            new Mock<ILogger<RepeatService>>().Object
            );
        const ulong guildId = 11;

        await service.InitAsync(guildId);
        await service.SetRepeatingAsync(guildId, true);

        Assert.True(await service.IsRepeatingAsync(guildId));
    }

    [Fact]
    public async Task SetRepeating_WithoutInit_CreatesAndUpdatesState()
    {
        var service = new RepeatService(new InMemoryPlaybackStateRepository(), new InMemoryRepeatListRepository(), new Mock<ILogger<RepeatService>>().Object);
        const ulong guildId = 12;

        await service.SetRepeatingAsync(guildId, true);

        Assert.True(await service.IsRepeatingAsync(guildId));
    }

    [Fact]
    public async Task SetRepeatingList_AfterInit_UpdatesFlag()
    {
        var service = new RepeatService(new InMemoryPlaybackStateRepository(), new InMemoryRepeatListRepository(), new Mock<ILogger<RepeatService>>().Object);
        const ulong guildId = 13;

        await service.InitAsync(guildId);
        await service.SetRepeatingListAsync(guildId, true);

        Assert.True(await service.IsRepeatingListAsync(guildId));
    }

    [Fact]
    public async Task SetRepeatingList_WithoutInit_CreatesAndUpdatesState()
    {
        var service = new RepeatService(new InMemoryPlaybackStateRepository(), new InMemoryRepeatListRepository(), new Mock<ILogger<RepeatService>>().Object);
        const ulong guildId = 14;

        await service.SetRepeatingListAsync(guildId, true);

        Assert.True(await service.IsRepeatingListAsync(guildId));
    }

    [Fact]
    public async Task SetRepeatingList_WhenDisabled_ClearsStoredRepeatList()
    {
        var playbackStateRepository = new InMemoryPlaybackStateRepository();
        var repeatListRepositoryMock = new Mock<IRepeatListRepository>();
        var service = new RepeatService(
            playbackStateRepository, 
            repeatListRepositoryMock.Object,
            new Mock<ILogger<RepeatService>>().Object);
        const ulong guildId = 15;

        await service.SetRepeatingListAsync(guildId, false);

        repeatListRepositoryMock.Verify(r => r.ClearAsync(guildId, default), Times.Once);
    }

    [Fact]
    public async Task SaveRepeatListSnapshot_WithCurrentTrack_PersistsCurrentThenQueue()
    {
        var playbackStateRepository = new InMemoryPlaybackStateRepository();
        var repeatListRepositoryMock = new Mock<IRepeatListRepository>();
        var service = new RepeatService(
            playbackStateRepository, 
            repeatListRepositoryMock.Object,
            new Mock<ILogger<RepeatService>>().Object);
        const ulong guildId = 16;

        var current = new Mock<ILavaLinkTrack>();
        current.Setup(t => t.ToString()).Returns("current-id");

        var queued1 = new Mock<ILavaLinkTrack>();
        queued1.Setup(t => t.ToString()).Returns("q1");
        var queued2 = new Mock<ILavaLinkTrack>();
        queued2.Setup(t => t.ToString()).Returns("q2");

        await service.SaveRepeatListSnapshotAsync(guildId, current.Object, new[] { queued1.Object, queued2.Object });

        repeatListRepositoryMock.Verify(r => r.ReplaceAsync(
                guildId,
                It.Is<IReadOnlyList<string>>(ids => ids.Count == 3 && ids[0] == "current-id" && ids[1] == "q1" && ids[2] == "q2"),
                default),
            Times.Once);
    }

    [Fact]
    public async Task SaveRepeatListSnapshot_WhenQueuedTracksNull_ThrowsArgumentNullException()
    {
        var service = new RepeatService(
            new InMemoryPlaybackStateRepository(), 
            new InMemoryRepeatListRepository(),
            new Mock<ILogger<RepeatService>>().Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.SaveRepeatListSnapshotAsync(17UL, null, null!));
    }

    [Fact]
    public async Task GetRepeatableQueueAsync_WhenStoredIdsExist_ReturnsParsedTracks()
    {
        var playbackStateRepository = new InMemoryPlaybackStateRepository();
        var repeatListRepositoryMock = new Mock<IRepeatListRepository>();
        repeatListRepositoryMock
            .Setup(r => r.GetTrackIdentifiersAsync(18UL, default))
            .ReturnsAsync(new[]
            {
                "QAAA2QMAPFJpY2sgQXN0bGV5IC0gTmV2ZXIgR29ubmEgR2l2ZSBZb3UgVXAgKE9mZmljaWFsIE11c2ljIFZpZGVvKQALUmljayBBc3RsZXkAAAAAAANACAALZFF3NHc5V2dYY1EAAQAraHR0cHM6Ly93d3cueW91dHViZS5jb20vd2F0Y2g/dj1kUXc0dzlXZ1hjUQEANGh0dHBzOi8vaS55dGltZy5jb20vdmkvZFF3NHc5V2dYY1EvbWF4cmVzZGVmYXVsdC5qcGcAAAd5b3V0dWJlAAAAAAAAAAA="
            });

        var service = new RepeatService(
            playbackStateRepository,
            repeatListRepositoryMock.Object,
            new Mock<ILogger<RepeatService>>().Object);

        var queue = await service.GetRepeatableQueueAsync(18UL);

        Assert.Single(queue);
    }

    private sealed class InMemoryPlaybackStateRepository : IPlaybackStateRepository
    {
        private readonly Dictionary<ulong, PlaybackStateRecord> _states = [];

        public Task<PlaybackStateRecord> GetOrCreateAsync(ulong guildId, CancellationToken cancellationToken = default)
        {
            if (!_states.TryGetValue(guildId, out var state))
            {
                state = new PlaybackStateRecord(guildId, false, false, null, null, DateTimeOffset.UtcNow);
                _states[guildId] = state;
                _states[guildId] = state;
            }

            return Task.FromResult(state);
        }

        public Task SetRepeatStateAsync(ulong guildId, bool isRepeating, bool isRepeatingList, CancellationToken cancellationToken = default)
        {
            _states[guildId] = new PlaybackStateRecord(guildId, isRepeating, isRepeatingList, null, null, DateTimeOffset.UtcNow);
            return Task.CompletedTask;
        }

        public Task SetCurrentTrackAsync(ulong guildId, string? trackIdentifier, long? queueItemId, CancellationToken cancellationToken = default)
        {
            var state = _states.GetValueOrDefault(guildId, new PlaybackStateRecord(guildId, false, false, null, null, DateTimeOffset.UtcNow));
            _states[guildId] = state with { CurrentTrackIdentifier = trackIdentifier, UpdatedAtUtc = DateTimeOffset.UtcNow };
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task GetRepeatableQueueAsync_WhenSomeIdentifiersInvalid_SkipsInvalidAndReturnsValid()
    {
        var playbackStateRepository = new InMemoryPlaybackStateRepository();
        var repeatListRepositoryMock = new Mock<IRepeatListRepository>();
        repeatListRepositoryMock
            .Setup(r => r.GetTrackIdentifiersAsync(19UL, default))
            .ReturnsAsync(new[]
            {
                "bad-identifier",
                "QAAA2QMAPFJpY2sgQXN0bGV5IC0gTmV2ZXIgR29ubmEgR2l2ZSBZb3UgVXAgKE9mZmljaWFsIE11c2ljIFZpZGVvKQALUmljayBBc3RsZXkAAAAAAANACAALZFF3NHc5V2dYY1EAAQAraHR0cHM6Ly93d3cueW91dHViZS5jb20vd2F0Y2g/dj1kUXc0dzlXZ1hjUQEANGh0dHBzOi8vaS55dGltZy5jb20vdmkvZFF3NHc5V2dYY1EvbWF4cmVzZGVmYXVsdC5qcGcAAAd5b3V0dWJlAAAAAAAAAAA="
            });

        var service = new RepeatService(
            playbackStateRepository,
            repeatListRepositoryMock.Object,
            new Mock<ILogger<RepeatService>>().Object);

        var result = await service.GetRepeatableQueueAsync(19UL);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetRepeatableQueueAsync_WhenAllIdentifiersInvalid_ReturnsEmpty()
    {
        var playbackStateRepository = new InMemoryPlaybackStateRepository();
        var repeatListRepositoryMock = new Mock<IRepeatListRepository>();
        repeatListRepositoryMock
            .Setup(r => r.GetTrackIdentifiersAsync(20UL, default))
            .ReturnsAsync(new[] { "bad-1", "bad-2" });

        var service = new RepeatService(
            playbackStateRepository,
            repeatListRepositoryMock.Object,
            new Mock<ILogger<RepeatService>>().Object);

        var result = await service.GetRepeatableQueueAsync(20UL);

        Assert.Empty(result);
    }

    [Fact]
    public async Task SaveRepeatListSnapshot_WithNullCurrentTrack_PersistsOnlyQueue()
    {
        var playbackStateRepository = new InMemoryPlaybackStateRepository();
        var repeatListRepositoryMock = new Mock<IRepeatListRepository>();
        var service = new RepeatService(
            playbackStateRepository,
            repeatListRepositoryMock.Object,
            new Mock<ILogger<RepeatService>>().Object);

        var queued1 = new Mock<ILavaLinkTrack>();
        queued1.Setup(t => t.ToString()).Returns("q1");

        await service.SaveRepeatListSnapshotAsync(21UL, null, new[] { queued1.Object });

        repeatListRepositoryMock.Verify(r => r.ReplaceAsync(
                21UL,
                It.Is<IReadOnlyList<string>>(ids => ids.Count == 1 && ids[0] == "q1"),
                default),
            Times.Once);
    }

    [Fact]
    public async Task SetRepeatingList_WhenEnabled_DoesNotClearRepeatList()
    {
        var repeatListRepositoryMock = new Mock<IRepeatListRepository>();
        var service = new RepeatService(
            new InMemoryPlaybackStateRepository(),
            repeatListRepositoryMock.Object,
            new Mock<ILogger<RepeatService>>().Object);

        await service.SetRepeatingListAsync(22UL, true);

        repeatListRepositoryMock.Verify(r => r.ClearAsync(It.IsAny<ulong>(), default), Times.Never);
    }

    private sealed class InMemoryRepeatListRepository : IRepeatListRepository
    {
        public Task<IReadOnlyList<string>> GetTrackIdentifiersAsync(ulong guildId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        }

        public Task ReplaceAsync(ulong guildId, IReadOnlyList<string> trackIdentifiers, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task ClearAsync(ulong guildId, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}