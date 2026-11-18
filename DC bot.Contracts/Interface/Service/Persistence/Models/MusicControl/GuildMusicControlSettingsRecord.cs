namespace DC_bot.Interface.Service.Persistence.Models.MusicControl;

/// <summary>Contains durable guild voting and DJ-role preferences.</summary>
/// <param name="VoteSkipEnabled">Whether ordinary skips require votes.</param>
/// <param name="DjRoleId">The optional role allowed to bypass voting.</param>
public sealed record GuildMusicControlSettingsRecord(bool VoteSkipEnabled, ulong? DjRoleId);
