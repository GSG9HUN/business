using System.Collections.Generic;
using DSharpPlus.Lavalink;

namespace DC_bot.Services;

public class MusicQueueService
{
    private readonly Queue<LavalinkTrack> _queue = new();
    public bool HasTracks => _queue.Count > 0;

    public void Enqueue(LavalinkTrack track)
    {
        _queue.Enqueue(track);
    }

    public LavalinkTrack? Dequeue()
    {
        return _queue.Count > 0 ? _queue.Dequeue() : null;
    }

    public IReadOnlyCollection<LavalinkTrack> ViewQueue()
    {
        return _queue;
    }
}