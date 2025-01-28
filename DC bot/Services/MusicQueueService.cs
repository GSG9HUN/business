using System.Collections.Generic;
using DSharpPlus.Lavalink;

namespace DC_bot.Services;

public class MusicQueueService
{
    private readonly Queue<LavalinkTrack> _queue = new();
    public Queue<LavalinkTrack> repeatableQueue = new();
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

    public void Clone(LavalinkTrack currentTrack)
    {
        repeatableQueue.Clear();
        repeatableQueue.Enqueue(currentTrack);
        foreach (var track in _queue)
        {
            repeatableQueue.Enqueue(track);
        }
    }
}