namespace DC_bot.Interface.Service.Music.MusicServiceInterface;

public interface IMusicQueueService
{
    public Task<bool> HasTracks(ulong guildId);

    public Task Enqueue(ulong guildId, ILavaLinkTrack track);
    public Task EnqueueMany(ulong guildId, IReadOnlyCollection<ILavaLinkTrack> tracks);
    public Task<ILavaLinkTrack?> Dequeue(ulong guildId);

    public Task<IReadOnlyCollection<ILavaLinkTrack>> ViewQueue(ulong guildId);
    

    public Task<Queue<ILavaLinkTrack>> GetQueue(ulong guildId);

    public Task SetQueue(ulong guildId, Queue<ILavaLinkTrack> shuffledQueue);
    IEnumerable<ILavaLinkTrack> GetRepeatableQueue(ulong guildId);
    Task ClearQueue(ulong guildId);
}