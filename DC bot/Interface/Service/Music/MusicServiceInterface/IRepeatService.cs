namespace DC_bot.Interface.Service.Music.MusicServiceInterface;

public interface IRepeatService
{
    public void Init(ulong guildId);
    public bool IsRepeating(ulong guildId);
    public void SetRepeating(ulong guildId, bool value);
    public bool IsRepeatingList(ulong guildId);
    public void SetRepeatingList(ulong guildId, bool value);
}