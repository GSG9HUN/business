using System.Text.Json;
using DC_bot.Helper;
using DC_bot.Interface;
using DC_bot.Service;
using Lavalink4NET.Tracks;
using Moq;

namespace DC_bot_tests.UnitTests.ServiceTest;

public class MusicQueueServiceTests
{
    private readonly string _tempQueueDirectory;

    public MusicQueueServiceTests()
    {
        // Ideiglenes könyvtár létrehozása a tesztekhez
        _tempQueueDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempQueueDirectory);

        // Beállítjuk a statikus QueueDirectory-t az ideiglenes könyvtárra
        MusicQueueService.QueueDirectory = _tempQueueDirectory;
    }

    [Fact]
    public void Constructor_CreatesQueueDirectory_WhenNotExists()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        MusicQueueService.QueueDirectory = tempDir;

        // Act
        var musicQueueService = new MusicQueueService();

        // Assert
        Assert.True(Directory.Exists(tempDir));

        // Cleanup
        if (Directory.Exists(tempDir))
            Directory.Delete(tempDir, true);
    }

    [Fact]
    public void Enqueue_AddsTrackToQueue_AndSavesToFile()
    {
        // Arrange
        var service = new MusicQueueService();
        const ulong guildId = 12345UL;

        var mockTrack = new Mock<ILavaLinkTrack>();
        mockTrack.Setup(t => t.ToLavalinkTrack())
            .Returns(new LavalinkTrack
            {
                Title = "test-track",
                Identifier = "Test Title",
                Author = "Test Author",
            });

        mockTrack.Setup(t => t.ToString()).Returns(
            "QAAA2QMAPFJpY2sgQXN0bGV5IC0gTmV2ZXIgR29ubmEgR2l2ZSBZb3UgVXAgKE9mZmljaWFsIE11c2ljIFZpZGVvKQALUmljayBBc3RsZXkAAAAAAANACAALZFF3NHc5V2dYY1EAAQAraHR0cHM6Ly93d3cueW91dHViZS5jb20vd2F0Y2g/dj1kUXc0dzlXZ1hjUQEANGh0dHBzOi8vaS55dGltZy5jb20vdmkvZFF3NHc5V2dYY1EvbWF4cmVzZGVmYXVsdC5qcGcAAAd5b3V0dWJlAAAAAAAAAAA=");

        // Act
        service.Enqueue(guildId, mockTrack.Object);

        // Assert
        Assert.True(service.HasTracks(guildId));

        var filePath = Path.Combine(_tempQueueDirectory, $"{guildId}.json");
        Assert.True(File.Exists(filePath));

        var savedTracks = JsonSerializer.Deserialize<List<SerializedTrack>>(File.ReadAllText(filePath));
        Assert.NotNull(savedTracks);
        Assert.Single(savedTracks);
        Assert.Equal(
            "QAAA2QMAPFJpY2sgQXN0bGV5IC0gTmV2ZXIgR29ubmEgR2l2ZSBZb3UgVXAgKE9mZmljaWFsIE11c2ljIFZpZGVvKQALUmljayBBc3RsZXkAAAAAAANACAALZFF3NHc5V2dYY1EAAQAraHR0cHM6Ly93d3cueW91dHViZS5jb20vd2F0Y2g/dj1kUXc0dzlXZ1hjUQEANGh0dHBzOi8vaS55dGltZy5jb20vdmkvZFF3NHc5V2dYY1EvbWF4cmVzZGVmYXVsdC5qcGcAAAd5b3V0dWJlAAAAAAAAAAA=",
            savedTracks[0].Identifier);
    }

    [Fact]
    public void Dequeue_RemovesTrackFromQueue_AndUpdatesFile()
    {
        // Arrange
        var service = new MusicQueueService();
        const ulong guildId = 12345UL;

        var mockTrack = new Mock<ILavaLinkTrack>();
        mockTrack.Setup(t => t.ToLavalinkTrack())
            .Returns(new LavalinkTrack
            {
                Title = "test-track",
                Identifier = "Test Title",
                Author = "Test Author"
            });

        mockTrack.Setup(t => t.ToString()).Returns(
            "QAAA2QMAPFJpY2sgQXN0bGV5IC0gTmV2ZXIgR29ubmEgR2l2ZSBZb3UgVXAgKE9mZmljaWFsIE11c2ljIFZpZGVvKQALUmljayBBc3RsZXkAAAAAAANACAALZFF3NHc5V2dYY1EAAQAraHR0cHM6Ly93d3cueW91dHViZS5jb20vd2F0Y2g/dj1kUXc0dzlXZ1hjUQEANGh0dHBzOi8vaS55dGltZy5jb20vdmkvZFF3NHc5V2dYY1EvbWF4cmVzZGVmYXVsdC5qcGcAAAd5b3V0dWJlAAAAAAAAAAA=");

        service.Enqueue(guildId, mockTrack.Object);

        // Act
        var dequeuedTrack = service.Dequeue(guildId);

        // Assert
        Assert.NotNull(dequeuedTrack);

        var filePath = Path.Combine(_tempQueueDirectory, $"{guildId}.json");
        var savedTracks = JsonSerializer.Deserialize<List<SerializedTrack>>(File.ReadAllText(filePath));

        Assert.Empty(savedTracks!);
    }

    [Fact]
    public async Task LoadQueue_LoadsTracksFromFile()
    {
        // Arrange
        var service = new MusicQueueService();
        var guildId = 12345UL;

        var savedTracks = new List<SerializedTrack>
        {
            new()
            {
                Identifier =
                    "QAAA2QMAPFJpY2sgQXN0bGV5IC0gTmV2ZXIgR29ubmEgR2l2ZSBZb3UgVXAgKE9mZmljaWFsIE11c2ljIFZpZGVvKQALUmljayBBc3RsZXkAAAAAAANACAALZFF3NHc5V2dYY1EAAQAraHR0cHM6Ly93d3cueW91dHViZS5jb20vd2F0Y2g/dj1kUXc0dzlXZ1hjUQEANGh0dHBzOi8vaS55dGltZy5jb20vdmkvZFF3NHc5V2dYY1EvbWF4cmVzZGVmYXVsdC5qcGcAAAd5b3V0dWJlAAAAAAAAAAA="
            }
        };

        var filePath = Path.Combine(_tempQueueDirectory, $"{guildId}.json");
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(savedTracks));

        // Act
        await service.LoadQueue(guildId);

        // Assert
        Assert.True(service.HasTracks(guildId));

        var queue = service.ViewQueue(guildId);
        Assert.Single(queue);
    }

    [Fact]
    public void Clone_CreatesRepeatableQueueWithCurrentTrackAndExistingTracks()
    {
        // Arrange
        var service = new MusicQueueService();
        const ulong guildId = 12345UL;
        service.Init(guildId);

        var currentTrack = new Mock<ILavaLinkTrack>();
        currentTrack.Setup(t => t.ToLavalinkTrack())
            .Returns(new LavalinkTrack
            {
                Title = "current-track",
                Identifier = "Current Title",
                Author = "Current Author"
            });

        var nextTrack = new Mock<ILavaLinkTrack>();
        nextTrack.Setup(t => t.ToLavalinkTrack())
            .Returns(new LavalinkTrack
            {
                Title = "next-track",
                Identifier = "Next Title",
                Author = "Next Author"
            });

        service.Enqueue(guildId, nextTrack.Object);

        // Act
        service.Clone(guildId, currentTrack.Object.ToLavalinkTrack());

        // Assert
        var repeatableQueue = service.GetRepeatableQueue(guildId).ToList();
        Assert.Equal(2, repeatableQueue.Count);
        Assert.Equal("Current Title", repeatableQueue[0].ToLavalinkTrack().Identifier);
        Assert.Equal("Next Title", repeatableQueue[1].ToLavalinkTrack().Identifier);
    }


    [Fact]
    public void Init_CreatesEmptyQueues()
    {
        // Arrange
        var service = new MusicQueueService();
        var guildId = 12345UL;

        // Act
        service.Init(guildId);

        // Assert
        var queue = service.GetQueue(guildId);
        var repeatableQueue = service.GetRepeatableQueue(guildId);

        Assert.Empty(queue);
        Assert.Empty(repeatableQueue);
    }

    [Fact]
    public void GetQueue_ReturnsCorrectTracks()
    {
        // Arrange
        var service = new MusicQueueService();
        const ulong guildId = 12345UL;

        var track1 = new Mock<ILavaLinkTrack>();
        track1.Setup(t => t.ToLavalinkTrack())
            .Returns(new LavalinkTrack
            {
                Title = "track1",
                Identifier = "Title 1",
                Author = "Author 1"
            });

        var track2 = new Mock<ILavaLinkTrack>();
        track2.Setup(t => t.ToLavalinkTrack())
            .Returns(new LavalinkTrack
            {
                Title = "track2",
                Identifier = "Title 2",
                Author = "Author 2"
            });

        service.Enqueue(guildId, track1.Object);
        service.Enqueue(guildId, track2.Object);

        // Act
        var queue = service.GetQueue(guildId);

        // Assert
        Assert.Equal(2, queue.Count);
        Assert.Equal("Title 1", queue.ElementAt(0).ToLavalinkTrack().Identifier);
        Assert.Equal("Title 2", queue.ElementAt(1).ToLavalinkTrack().Identifier);
    }

    [Fact]
    public void SetQueue_UpdatesTheQueueCorrectly()
    {
        // Arrange
        var service = new MusicQueueService();
        const ulong guildId = 12345UL;

        var track1 = new Mock<ILavaLinkTrack>();
        track1.Setup(t => t.ToLavalinkTrack())
            .Returns(new LavalinkTrack
            {
                Title = "track1",
                Identifier = "Title 1",
                Author = "Author 1"
            });

        var track2 = new Mock<ILavaLinkTrack>();
        track2.Setup(t => t.ToLavalinkTrack())
            .Returns(new LavalinkTrack
            {
                Title = "track2",
                Identifier = "Title 2",
                Author = "Author 2"
            });

        var shuffledQueue = new Queue<ILavaLinkTrack>([track2.Object, track1.Object]);

        // Act
        service.SetQueue(guildId, shuffledQueue);

        // Assert
        var queue = service.GetQueue(guildId);

        Assert.Equal(2, queue.Count);
        Assert.Equal("Title 2", queue.ElementAt(0).ToLavalinkTrack().Identifier);
        Assert.Equal("Title 1", queue.ElementAt(1).ToLavalinkTrack().Identifier);
    }

    [Fact]
    public void GetRepeatableQueue_ReturnsCorrectTracks()
    {
        // Arrange
        var service = new MusicQueueService();
        var guildId = 12345UL;
        service.Init(guildId);

        var currentTrack = new Mock<ILavaLinkTrack>();
        currentTrack.Setup(t => t.ToLavalinkTrack())
            .Returns(new LavalinkTrack
            {
                Title = "current-track",
                Identifier = "Current Title",
                Author = "Current Author"
            });

        var nextTrack = new Mock<ILavaLinkTrack>();
        nextTrack.Setup(t => t.ToLavalinkTrack())
            .Returns(new LavalinkTrack
            {
                Title = "next-track",
                Identifier = "Next Title",
                Author = "Next Author"
            });

        service.Enqueue(guildId, nextTrack.Object);

        // Act
        service.Clone(guildId, currentTrack.Object.ToLavalinkTrack());

        // Assert
        var repeatableQueue = service.GetRepeatableQueue(guildId).ToList();

        Assert.Equal(2, repeatableQueue.Count);
    }

    [Fact]
    public void Dequeue_ReturnsNullWhenItsEmpty()
    {
        //Arrange
        var service = new MusicQueueService();
        var guildId = 12345UL;
        service.Init(guildId);

        // Act
        var track = service.Dequeue(guildId);
        // Assert
        Assert.Null(track);
    }

    [Fact]
    public async Task LoadQueue_DoesNotAddTracks_WhenSavedTracksIsNull()
    {
        // Arrange
        var service = new MusicQueueService();
        const ulong guildId = 12345UL;

        var filePath = Path.Combine(MusicQueueService.QueueDirectory, $"{guildId}.json");
        File.WriteAllText(filePath, JsonSerializer.Serialize<List<SerializedTrack>>(null)); // Null érték mentése

        // Act
        await service.LoadQueue(guildId);

        // Assert
        Assert.False(service.HasTracks(guildId)); // Nem szabad, hogy a queue-ban legyenek elemek
    }

    [Fact]
    public async Task LoadQueue_DoesNotAddTracks_WhenSavedTracksIsEmpty()
    {
        // Arrange
        var service = new MusicQueueService();
        var guildId = 12345UL;

        var filePath = Path.Combine(MusicQueueService.QueueDirectory, $"{guildId}.json");
        File.WriteAllText(filePath, JsonSerializer.Serialize(new List<SerializedTrack>())); // Üres lista mentése

        // Act
        await service.LoadQueue(guildId);

        // Assert
        Assert.False(service.HasTracks(guildId)); // Nem szabad, hogy a queue-ban legyenek elemek
    }

    [Fact]
    public async Task LoadQueue_DoesNothing_WhenFileDoesNotExist()
    {
        // Arrange
        var service = new MusicQueueService();
        var guildId = 12345UL;

        var filePath = Path.Combine(MusicQueueService.QueueDirectory, $"{guildId}.json");

        // Biztosítjuk, hogy a fájl nem létezik
        File.Delete(filePath);

        // Act
        await service.LoadQueue(guildId);

        // Assert
        Assert.False(service.HasTracks(guildId)); // Nem szabad, hogy a queue-ban legyenek elemek
    }

    [Fact]
    public void Enqueue_WithoutInit_ShouldStillWork()
    {
        // Arrange
        var service = new MusicQueueService();
        const ulong guildId = 12345UL;

        var mockTrack = new Mock<ILavaLinkTrack>();
        mockTrack.Setup(t => t.ToLavalinkTrack())
            .Returns(new LavalinkTrack
            {
                Title = "test-track",
                Identifier = "Test Identifier",
                Author = "Test Author"
            });

        mockTrack.Setup(t => t.ToString()).Returns(
            "QAAA2QMAPFJpY2sgQXN0bGV5IC0gTmV2ZXIgR29ubmEgR2l2ZSBZb3UgVXAgKE9mZmljaWFsIE11c2ljIFZpZGVvKQALUmljayBBc3RsZXkAAAAAAANACAALZFF3NHc5V2dYY1EAAQAraHR0cHM6Ly93d3cueW91dHViZS5jb20vd2F0Y2g/dj1kUXc0dzlXZ1hjUQEANGh0dHBzOi8vaS55dGltZy5jb20vdmkvZFF3NHc5V2dYY1EvbWF4cmVzZGVmYXVsdC5qcGcAAAd5b3V0dWJlAAAAAAAAAAA=");

        // Act - Enqueue without calling Init()
        service.Enqueue(guildId, mockTrack.Object);

        // Assert
        Assert.True(service.HasTracks(guildId));
        var queue = service.GetQueue(guildId);
        Assert.Single(queue);
    }

    [Fact]
    public void Clone_WhenRepeatableQueueNotInitialized_ShouldThrow()
    {
        // Arrange
        var service = new MusicQueueService();
        const ulong guildId = 12345UL;

        var currentTrack = new LavalinkTrack
        {
            Title = "current-track",
            Identifier = "Current Title",
            Author = "Current Author"
        };

        // Act & Assert - Clone without calling Init() should throw KeyNotFoundException
        Assert.Throws<KeyNotFoundException>(() => service.Clone(guildId, currentTrack));
    }

    [Fact]
    public void HasTracks_WhenGuildNotRegistered_ShouldReturnFalse()
    {
        // Arrange
        var service = new MusicQueueService();
        const ulong unknownGuildId = 99999UL;

        // Act
        var hasTracks = service.HasTracks(unknownGuildId);

        // Assert
        Assert.False(hasTracks);
    }

    [Fact]
    public async Task LoadQueue_WithInvalidJson_ShouldThrow()
    {
        // Arrange
        var service = new MusicQueueService();
        const ulong guildId = 12345UL;

        var filePath = Path.Combine(MusicQueueService.QueueDirectory, $"{guildId}.json");
        await File.WriteAllTextAsync(filePath, "{ invalid json content :::"); // Invalid JSON

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(async () => await service.LoadQueue(guildId));
    }
}