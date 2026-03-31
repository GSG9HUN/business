using DC_bot.Interface.Service.Music.MusicServiceInterface;

namespace DC_bot.Service.Music.MusicServices;

public class RepeatService : IRepeatService
{
    private readonly Dictionary<ulong, bool> _isRepeating = new();
    private readonly Dictionary<ulong, bool> _isRepeatingList = new();

    public void Init(ulong guildId)
    {
        _isRepeating.TryAdd(guildId, false);
        _isRepeatingList.TryAdd(guildId, false);
    }

    public bool IsRepeating(ulong guildId)
    {
        return _isRepeating.TryGetValue(guildId, out var repeating) && repeating;
    }

    public void SetRepeating(ulong guildId, bool value)
    {
        if (_isRepeating.ContainsKey(guildId)) _isRepeating[guildId] = value;
    }

    public bool IsRepeatingList(ulong guildId)
    {
        return _isRepeatingList.TryGetValue(guildId, out var repeatingList) && repeatingList;
    }

    public void SetRepeatingList(ulong guildId, bool value)
    {
        if (_isRepeatingList.ContainsKey(guildId)) _isRepeatingList[guildId] = value;
    }
}