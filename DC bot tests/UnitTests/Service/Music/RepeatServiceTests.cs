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