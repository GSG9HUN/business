using DSharpPlus.Lavalink;

namespace DC_bot.Interface;

public interface IMusicQueueService
{
    public bool HasTracks(ulong guildId);

    public void Enqueue(ulong guildId, ILavaLinkTrack track);
    public LavalinkTrack? Dequeue(ulong guildId);

    public IReadOnlyCollection<ILavaLinkTrack> ViewQueue(ulong guildId);
    public Task LoadQueue(ulong guildId, LavalinkRestClient nodeRest);

    public void Clone(ulong guildId, LavalinkTrack currentTrack);

    public void Init(ulong guildId);

    public Queue<ILavaLinkTrack> GetQueue(ulong guildId);

    public void SetQueue(ulong guildId, Queue<ILavaLinkTrack> shuffledQueue);
    IEnumerable<ILavaLinkTrack> GetRepeatableQueue(ulong guildId);
}