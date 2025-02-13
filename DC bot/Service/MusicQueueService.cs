using System.Text.Json;
using DC_bot.Helper;
using DSharpPlus.Lavalink;

namespace DC_bot.Service;

public class MusicQueueService
{
    private readonly Dictionary<ulong, Queue<LavalinkTrack>> _queues = new();
    public readonly Dictionary<ulong, Queue<LavalinkTrack>> RepeatableQueue = new();

    private static readonly string QueueDirectory =
        Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName ?? throw new InvalidOperationException(), "queues");

    public bool HasTracks(ulong guildId) => _queues.ContainsKey(guildId) && _queues[guildId].Count > 0;

    public MusicQueueService()
    {
        if (!Directory.Exists(QueueDirectory))
            Directory.CreateDirectory(QueueDirectory);
    }

    public void Enqueue(ulong guildId, LavalinkTrack track)
    {
        if (!_queues.ContainsKey(guildId))
            _queues[guildId] = new Queue<LavalinkTrack>();

        _queues[guildId].Enqueue(track);
        SaveQueue(guildId);
    }

    public LavalinkTrack? Dequeue(ulong guildId)
    {
        if (!HasTracks(guildId)) return null;

        var track = _queues[guildId].Dequeue();
        SaveQueue(guildId);
        return track;
    }

    public IReadOnlyCollection<LavalinkTrack> ViewQueue(ulong guildId)
    {
        return _queues.TryGetValue(guildId, out var queue) ? queue : new List<LavalinkTrack>();
    }

    private void SaveQueue(ulong guildId)
    {
        var filePath = Path.Combine(QueueDirectory, $"{guildId}.json");

        if (!_queues.TryGetValue(guildId, out var queue)) return;

        var tracks = queue
            .Select(track => new SerializedTrack { TrackString = track.TrackString })
            .ToList();

        File.WriteAllText(filePath, JsonSerializer.Serialize(tracks));
    }

    public async Task LoadQueue(ulong guildId, LavalinkRestClient nodeRest)
    {
        var filePath = Path.Combine(QueueDirectory, $"{guildId}.json");

        if (!File.Exists(filePath)) return;

        var savedTracks = JsonSerializer.Deserialize<List<SerializedTrack>>(File.ReadAllText(filePath));

        if (savedTracks == null) return;

        _queues[guildId] = new Queue<LavalinkTrack>();

        foreach (var track in savedTracks)
        {
            var decodedTracks = await nodeRest.DecodeTracksAsync(new List<string> { track.TrackString });
            if (decodedTracks == null) continue;
            _queues[guildId].Enqueue(decodedTracks.First());
        }
    }

    public void Clone(ulong guildId, LavalinkTrack currentTrack)
    {
        RepeatableQueue[guildId].Clear();
        RepeatableQueue[guildId].Enqueue(currentTrack);
        foreach (var track in _queues[guildId])
        {
            RepeatableQueue[guildId].Enqueue(track);
        }
    }

    public void Init(ulong guildId)
    {
        _queues.Add(guildId, new Queue<LavalinkTrack>());
        RepeatableQueue.Add(guildId, new Queue<LavalinkTrack>());
    }
}