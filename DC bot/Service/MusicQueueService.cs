using System.Text.Json;
using DC_bot.Helper;
using DC_bot.Interface;
using DC_bot.Wrapper;
using DSharpPlus.Lavalink;

namespace DC_bot.Service;

public class MusicQueueService
{
    private readonly Dictionary<ulong, Queue<ILavaLinkTrack>> _queues = new();
    public readonly Dictionary<ulong, Queue<ILavaLinkTrack>> RepeatableQueue = new();

    private static readonly string QueueDirectory =
        Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName ?? throw new InvalidOperationException(), "queues");

    public bool HasTracks(ulong guildId) => _queues.ContainsKey(guildId) && _queues[guildId].Count > 0;

    public MusicQueueService()
    {
        if (!Directory.Exists(QueueDirectory))
            Directory.CreateDirectory(QueueDirectory);
    }

    public void Enqueue(ulong guildId, ILavaLinkTrack track)
    {
        if (!_queues.ContainsKey(guildId))
            _queues[guildId] = new Queue<ILavaLinkTrack>();

        _queues[guildId].Enqueue(track);
        SaveQueue(guildId);
    }

    public LavalinkTrack? Dequeue(ulong guildId)
    {
        if (!HasTracks(guildId)) return null;

        var track = _queues[guildId].Dequeue();
        SaveQueue(guildId);
        return track.ToLavalinkTrack();
    }

    public IReadOnlyCollection<ILavaLinkTrack> ViewQueue(ulong guildId)
    {
        return _queues.TryGetValue(guildId, out var queue) ? queue : new List<ILavaLinkTrack>();
    }

    private void SaveQueue(ulong guildId)
    {
        var filePath = Path.Combine(QueueDirectory, $"{guildId}.json");

        if (!_queues.TryGetValue(guildId, out var queue)) return;

        var tracks = queue
            .Select(track => new SerializedTrack { TrackString = track.ToLavalinkTrack().TrackString })
            .ToList();

        File.WriteAllText(filePath, JsonSerializer.Serialize(tracks));
    }

    public async Task LoadQueue(ulong guildId, LavalinkRestClient nodeRest)
    {
        var filePath = Path.Combine(QueueDirectory, $"{guildId}.json");

        if (!File.Exists(filePath)) return;

        var savedTracks = JsonSerializer.Deserialize<List<SerializedTrack>>(File.ReadAllText(filePath));

        if (savedTracks == null) return;

        _queues[guildId] = new Queue<ILavaLinkTrack>();

        foreach (var track in savedTracks)
        {
            var decodedTracks = await nodeRest.DecodeTracksAsync(new List<string> { track.TrackString });
            if (decodedTracks == null) continue;
            _queues[guildId].Enqueue(new LavaLinkTrackWrapper(decodedTracks.First()));
        }
    }

    public void Clone(ulong guildId, LavalinkTrack currentTrack)
    {
        RepeatableQueue[guildId].Clear();
        RepeatableQueue[guildId].Enqueue(new LavaLinkTrackWrapper(currentTrack));
        foreach (var track in _queues[guildId])
        {
            RepeatableQueue[guildId].Enqueue(track);
        }
    }

    public void Init(ulong guildId)
    {
        _queues.Add(guildId, new Queue<ILavaLinkTrack>());
        RepeatableQueue.Add(guildId, new Queue<ILavaLinkTrack>());
    }
}