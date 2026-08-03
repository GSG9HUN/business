namespace API.Responses;

public sealed class GuildSummaryResponse(ulong guildId, string name)
{
    public ulong GuildId { get; init; } = guildId;
    public string Name { get; init; } = name;

    public void Deconstruct(out ulong guildId, out string name)
    {
        guildId = GuildId;
        name = Name;
    }
}
