using DC_bot.Exceptions.Music;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Music;
using DC_bot.Service.Music.MusicServices;
using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.IntegrationTests.Service.Music;

public class LavaLinkServiceIntegrationTests
{
    private const ulong GuildId = 555UL;
    private const ulong VoiceChannelId = 777UL;
    private readonly Mock<IAudioService> _audioServiceMock = new();
    private readonly CurrentTrackService _currentTrackService;

    private readonly Mock<IDiscordGuild> _guildMock = new();
    private readonly Mock<ILocalizationService> _localizationMock = new();

    private readonly Mock<ILogger<LavaLinkService>> _loggerMock = new();
    private readonly Mock<IDiscordMember> _memberMock = new();
    private readonly Mock<IDiscordMessage> _messageMock = new();
    private readonly Mock<IPlaybackEventHandlerService> _playbackEventHandlerMock = new();
    private readonly Mock<IPlayerConnectionService> _playerConnectionMock = new();
    private readonly Mock<ILavalinkPlayer> _playerMock = new();
    private readonly Mock<IProgressiveTimerService> _progressiveTimerServiceMock = new();

    private const string ValidTrackIdentifier =
        "QAAA2QMAPFJpY2sgQXN0bGV5IC0gTmV2ZXIgR29ubmEgR2l2ZSBZb3UgVXAgKE9mZmljaWFsIE11c2ljIFZpZGVvKQALUmljayBBc3RsZXkAAAAAAANACAALZFF3NHc5V2dYY1EAAQAraHR0cHM6Ly93d3cueW91dHViZS5jb20vd2F0Y2g/dj1kUXc0dzlXZ1hjUQEANGh0dHBzOi8vaS55dGltZy5jb20vdmkvZFF3NHc5V2dYY1EvbWF4cmVzZGVmYXVsdC5qcGcAAAd5b3V0dWJlAAAAAAAAAAA=";
    private readonly InMemoryQueueRepository _inMemoryQueueRepository = new();
    private readonly MusicQueueService _queueService;
    private readonly RepeatService _repeatService;
    private readonly Mock<IResponseBuilder> _responseBuilderMock = new();

    private readonly LavaLinkService _service;
    private readonly Mock<IDiscordChannel> _textChannelMock = new();
    private readonly Mock<ITrackNotificationService> _trackNotificationMock = new();
    private readonly Mock<ITrackPlaybackService> _trackPlaybackMock = new();
    private readonly Mock<IDiscordChannel> _voiceChannelMock = new();
    private readonly Mock<IDiscordVoiceState> _voiceStateMock = new();

    public LavaLinkServiceIntegrationTests()
    {
        var inMemoryPlaybackStateRepository = new InMemoryPlaybackStateRepository();
        var inMemoryRepeatListRepository = new InMemoryRepeatListRepository();
        var repeatServiceLoggerMock = new Mock<ILogger<RepeatService>>();
        var currentTrackServiceLoggerMock = new Mock<ILogger<CurrentTrackService>>();
        
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        _queueService = new MusicQueueService(_inMemoryQueueRepository);
        _repeatService = new RepeatService(
            inMemoryPlaybackStateRepository,
            inMemoryRepeatListRepository,
            repeatServiceLoggerMock.Object);
        
        _currentTrackService = new CurrentTrackService(inMemoryPlaybackStateRepository, currentTrackServiceLoggerMock.Object);

        _guildMock.Setup(g => g.Id).Returns(GuildId);
        _voiceChannelMock.Setup(c => c.Id).Returns(VoiceChannelId);
        _voiceChannelMock.Setup(c => c.Guild).Returns(_guildMock.Object);
        _voiceChannelMock.Setup(c => c.Name).Returns("voice");

        _textChannelMock.Setup(c => c.Guild).Returns(_guildMock.Object);
        _textChannelMock.Setup(c => c.Name).Returns("text");

        _voiceStateMock.Setup(v => v.Channel).Returns(_voiceChannelMock.Object);
        _memberMock.Setup(m => m.VoiceState).Returns(_voiceStateMock.Object);

        _messageMock.SetupProperty(m => m.Channel, _textChannelMock.Object);

        _service = new LavaLinkService(
            _queueService,
            _loggerMock.Object,
            _audioServiceMock.Object,
            _responseBuilderMock.Object,
            _localizationMock.Object,
            _repeatService,
            _currentTrackService,
            _trackNotificationMock.Object,
            _playerConnectionMock.Object,
            _playbackEventHandlerMock.Object,
            _progressiveTimerServiceMock.Object,
            _trackPlaybackMock.Object);
    }

    [Fact]
    public async Task Init_InitializesCurrentTrackAndRepeatState()
    {
        await _service.Init(GuildId);

        Assert.Null(await _currentTrackService.GetCurrentTrackAsync(GuildId));
        Assert.False(await _repeatService.IsRepeatingAsync(GuildId));
        Assert.False(await _repeatService.IsRepeatingListAsync(GuildId));
    }

    [Fact]
    public async Task ConnectAsync_WhenCalledTwice_StartsAudioOnlyOnce()
    {
        _audioServiceMock.Setup(a => a.StartAsync(default)).Returns(new ValueTask());

        await _service.ConnectAsync();
        await _service.ConnectAsync();

        _audioServiceMock.Verify(a => a.StartAsync(default), Times.Once);
    }

    [Fact]
    public async Task ConnectAsync_WhenAudioStartFails_ThrowsLavalinkOperationException()
    {
        _audioServiceMock.Setup(a => a.StartAsync(default)).ThrowsAsync(new InvalidOperationException("boom"));

        await Assert.ThrowsAsync<LavalinkOperationException>(() => _service.ConnectAsync());
    }

    [Fact]
    public async Task StartPlayingQueue_WithQueuedTrack_PlaysNotifiesAndSetsCurrentTrack()
    {
        await _service.Init(GuildId);
        await _inMemoryQueueRepository.EnqueueAsync(GuildId, ValidTrackIdentifier);

        _playerConnectionMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        await _service.StartPlayingQueue(_messageMock.Object, _textChannelMock.Object, _memberMock.Object);

        _playbackEventHandlerMock.Verify(
            h => h.RegisterPlaybackFinishedHandler(GuildId, _playerMock.Object, _textChannelMock.Object), Times.Once);
        _playerMock.Verify(
            p => p.PlayAsync(It.IsAny<LavalinkTrack>(), It.IsAny<TrackPlayProperties>(), default),
            Times.Once);
        _trackNotificationMock.Verify(
            n => n.NotifyNowPlayingAsync(_textChannelMock.Object, It.IsAny<ILavaLinkTrack>(),
                It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()), Times.Once);

        var current = await _currentTrackService.GetCurrentTrackAsync(GuildId);
        Assert.NotNull(current);
    }

    [Fact]
    public async Task StartPlayingQueue_WhenQueueEmpty_DoesNotPlay()
    {
        await _service.Init(GuildId);

        _playerConnectionMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        await _service.StartPlayingQueue(_messageMock.Object, _textChannelMock.Object, _memberMock.Object);

        _playerMock.Verify(p => p.PlayAsync(It.IsAny<LavalinkTrack>(), It.IsAny<TrackPlayProperties>(), default),
            Times.Never);
        _trackNotificationMock.Verify(
            n => n.NotifyNowPlayingAsync(It.IsAny<IDiscordChannel>(), It.IsAny<ILavaLinkTrack>(), It.IsAny<TimeSpan>(),
                It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task LeaveVoiceChannel_WhenCurrentTrackExists_StopsCleansAndDisconnects()
    {
        _playerMock.Setup(p => p.CurrentTrack).Returns(new LavalinkTrack
        {
            Author = "Artist",
            Title = "Current",
            Identifier = "id-current"
        });

        _playerConnectionMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        await _service.LeaveVoiceChannel(_messageMock.Object, _memberMock.Object);

        _playerMock.Verify(p => p.StopAsync(default), Times.Once);
        _playbackEventHandlerMock.Verify(h => h.CleanupGuildAsync(GuildId), Times.Once);
        _playerMock.Verify(p => p.DisconnectAsync(default), Times.Once);
    }

    private sealed class InMemoryQueueRepository : IQueueRepository
    {
        private long _nextId = 1;
        private readonly Dictionary<ulong, List<QueueItemRecord>> _items = [];

        public Task<IReadOnlyList<QueueItemRecord>> GetQueuedItemsAsync(ulong guildId, CancellationToken cancellationToken = default)
        {
            var items = _items.GetValueOrDefault(guildId, [])
                .Where(item => item.State == 0)
                .OrderBy(item => item.Position)
                .ToList();

            return Task.FromResult<IReadOnlyList<QueueItemRecord>>(items);
        }

        public Task<bool> AnyQueuedItemsAsync(ulong guildId, CancellationToken cancellationToken = default)
        {
            var anyQueued = _items.GetValueOrDefault(guildId, []).Any(item => item.State == 0);
            return Task.FromResult(anyQueued);
        }

        public Task<QueueItemRecord?> GetNextQueuedItemAsync(ulong guildId, CancellationToken cancellationToken = default)
        {
            var item = _items.GetValueOrDefault(guildId, [])
                .Where(queueItem => queueItem.State == 0)
                .OrderBy(queueItem => queueItem.Position)
                .FirstOrDefault();

            return Task.FromResult(item);
        }

        public Task<QueueItemRecord?> GetPreviousItemAsync(ulong guildId, CancellationToken cancellationToken = default)
        {
            var item = _items.GetValueOrDefault(guildId, [])
                .Where(queueItem => queueItem.State == 2 || queueItem.State == 3)
                .OrderByDescending(queueItem => queueItem.Position)
                .FirstOrDefault();

            return Task.FromResult(item);
        }

        public Task<QueueItemRecord?> ClaimNextQueuedItemAsync(ulong guildId, CancellationToken cancellationToken = default)
        {
            var items = _items.GetValueOrDefault(guildId, []);
            var index = items
                .Select((item, idx) => (item, idx))
                .Where(x => x.item.State == 0)
                .OrderBy(x => x.item.Position)
                .Select(x => x.idx)
                .FirstOrDefault(-1);

            if (index < 0)
            {
                return Task.FromResult<QueueItemRecord?>(null);
            }

            items[index] = items[index] with { State = 1 };
            return Task.FromResult<QueueItemRecord?>(items[index]);
        }

        public Task<QueueItemRecord> EnqueueAsync(ulong guildId, string trackIdentifier, CancellationToken cancellationToken = default)
        {
            var items = _items.GetValueOrDefault(guildId);
            if (items is null)
            {
                items = [];
                _items[guildId] = items;
            }

            var nextPosition = items.Count == 0 ? 0 : items.Max(item => item.Position) + 1;
            var record = new QueueItemRecord(_nextId++, guildId, nextPosition, trackIdentifier, 0, DateTimeOffset.UtcNow, null, null);
            items.Add(record);
            return Task.FromResult(record);
        }

        public async Task EnqueueManyAsync(ulong guildId, IReadOnlyList<string> trackIdentifiers, CancellationToken cancellationToken = default)
        {
            foreach (var trackIdentifier in trackIdentifiers)
            {
                await EnqueueAsync(guildId, trackIdentifier, cancellationToken);
            }
        }

        public Task ReorderQueuedItemsAsync(ulong guildId, IReadOnlyList<string> trackIdentifiers, CancellationToken cancellationToken = default)
        {
            var items = _items.GetValueOrDefault(guildId, []);
            var ordered = new List<QueueItemRecord>(trackIdentifiers.Count);

            foreach (var trackIdentifier in trackIdentifiers)
            {
                var item = items.First(queueItem => queueItem.TrackIdentifier == trackIdentifier && !ordered.Contains(queueItem));
                ordered.Add(item);
            }

            for (var index = 0; index < ordered.Count; index++)
            {
                items[items.IndexOf(ordered[index])] = ordered[index] with { Position = index };
            }

            return Task.CompletedTask;
        }

        public Task UpdateQueueItemPositionAsync(long queueItemId, int newPosition, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task MarkPlayingAsync(long queueItemId, CancellationToken cancellationToken = default)
        {
            Update(queueItemId, item => item with { State = 1 });
            return Task.CompletedTask;
        }

        public Task MarkPlayedAsync(long queueItemId, CancellationToken cancellationToken = default)
        {
            Update(queueItemId, item => item with { State = 2, PlayedAtUtc = DateTimeOffset.UtcNow });
            return Task.CompletedTask;
        }

        public Task MarkSkippedAsync(long queueItemId, CancellationToken cancellationToken = default)
        {
            Update(queueItemId, item => item with { State = 3, SkippedAtUtc = DateTimeOffset.UtcNow });
            return Task.CompletedTask;
        }

        public Task MarkAllQueuedAsSkippedAsync(ulong guildId, CancellationToken cancellationToken = default)
        {
            var items = _items.GetValueOrDefault(guildId, []);
            for (var index = 0; index < items.Count; index++)
            {
                if (items[index].State == 0)
                {
                    items[index] = items[index] with { State = 3, SkippedAtUtc = DateTimeOffset.UtcNow };
                }
            }

            return Task.CompletedTask;
        }

        private void Update(long queueItemId, Func<QueueItemRecord, QueueItemRecord> update)
        {
            foreach (var guildItems in _items.Values)
            {
                var index = guildItems.FindIndex(item => item.Id == queueItemId);
                if (index < 0)
                {
                    continue;
                }

                guildItems[index] = update(guildItems[index]);
                return;
            }
        }
    }

    private sealed class InMemoryPlaybackStateRepository : IPlaybackStateRepository
    {
        private readonly Dictionary<ulong, PlaybackStateRecord> _states = [];

        public Task<PlaybackStateRecord> GetOrCreateAsync(ulong guildId, CancellationToken cancellationToken = default)
        {
            if (_states.TryGetValue(guildId, out var state)) 
                return Task.FromResult(state);
            
            var newState = new PlaybackStateRecord(
                guildId, 
                false, 
                false, 
                null, 
                null, 
                DateTimeOffset.UtcNow);
            
            _states[guildId] = newState;
            return Task.FromResult(newState);
        }

        public Task SetRepeatStateAsync(ulong guildId, bool isRepeating, bool isRepeatingList, CancellationToken cancellationToken = default)
        {
            var state = _states.GetValueOrDefault(guildId, new PlaybackStateRecord(
                guildId, 
                false, 
                false, 
                null, 
                null,
                DateTimeOffset.UtcNow));

            _states[guildId] = state with
            {
                IsRepeating = isRepeating,
                IsRepeatingList = isRepeatingList,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };

            return Task.CompletedTask;
        }

        public Task SetCurrentTrackAsync(ulong guildId, string? trackIdentifier, long? queueItemId, CancellationToken cancellationToken = default)
        {
            
            var state = _states.GetValueOrDefault(guildId, new PlaybackStateRecord(
                guildId, 
                false, 
                false, 
                null, 
                null,
                DateTimeOffset.UtcNow));
    
            _states[guildId] = state with
            {
                CurrentTrackIdentifier = trackIdentifier,
                QueueItemId = queueItemId, 
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };

            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryRepeatListRepository : IRepeatListRepository
    {
        private readonly Dictionary<ulong, IReadOnlyList<string>> _lists = [];

        public Task<IReadOnlyList<string>> GetTrackIdentifiersAsync(ulong guildId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_lists.GetValueOrDefault(guildId, Array.Empty<string>()));
        }

        public Task ReplaceAsync(ulong guildId, IReadOnlyList<string> trackIdentifiers, CancellationToken cancellationToken = default)
        {
            _lists[guildId] = trackIdentifiers.ToList();
            return Task.CompletedTask;
        }

        public Task ClearAsync(ulong guildId, CancellationToken cancellationToken = default)
        {
            _lists.Remove(guildId);
            return Task.CompletedTask;
        }
    }
}