using System.Text.Json;
using DC_bot.Helper;
using DC_bot.Interface;
using DC_bot.Wrapper;
using Lavalink4NET;
using Lavalink4NET.Tracks;

namespace DC_bot.Service;

public class MusicQueueService : IMusicQueueService
{
    private readonly Dictionary<ulong, Queue<ILavaLinkTrack>> _queues = new();
    private readonly Dictionary<ulong, Queue<ILavaLinkTrack>> _repeatableQueue = new();

    internal static string QueueDirectory = Path.Combine(Directory.GetCurrentDirectory(), "guildFiles/queues");

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
            .Select(track => new SerializedTrack { Identifier = track.ToString() })
            .ToList();

        File.WriteAllText(filePath, JsonSerializer.Serialize(tracks));
    }

    public Task LoadQueue(ulong guildId, IAudioService nodeRest)
    {
        var filePath = Path.Combine(QueueDirectory, $"{guildId}.json");

        if (!File.Exists(filePath)) return Task.CompletedTask;
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true, // Ha a JSON kis-nagybetű érzékeny problémákat okozna
            WriteIndented = true
        };

        var savedTracks = JsonSerializer.Deserialize<List<SerializedTrack>>(File.ReadAllText(filePath), options);

        _queues[guildId] = new Queue<ILavaLinkTrack>();

        if (savedTracks == null || savedTracks.Count == 0) return Task.CompletedTask;

        var trackIdentifierList = savedTracks.Select(track => track.Identifier).ToList();
        foreach (var track in trackIdentifierList.Select(trackIdentifier => LavalinkTrack.Parse(trackIdentifier,null)))
        {
            _queues[guildId].Enqueue(new LavaLinkTrackWrapper(track));
        }

        return Task.CompletedTask;
    }

    public void Clone(ulong guildId, LavalinkTrack currentTrack)
    {
        _repeatableQueue[guildId].Clear();
        _repeatableQueue[guildId].Enqueue(new LavaLinkTrackWrapper(currentTrack));
        foreach (var track in _queues[guildId])
        {
            _repeatableQueue[guildId].Enqueue(track);
        }
    }

    public void Init(ulong guildId)
    {
        _queues.Add(guildId, new Queue<ILavaLinkTrack>());
        _repeatableQueue.Add(guildId, new Queue<ILavaLinkTrack>());
    }

    public Queue<ILavaLinkTrack> GetQueue(ulong guildId)
    {
        return _queues[guildId];
    }

    public void SetQueue(ulong guildId, Queue<ILavaLinkTrack> shuffledQueue)
    {
        _queues[guildId] = shuffledQueue;
    }

    public IEnumerable<ILavaLinkTrack> GetRepeatableQueue(ulong guildId)
    {
        return _repeatableQueue[guildId];
    }
}