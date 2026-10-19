namespace DC_bot.BotControl.Payloads;

public sealed record PlaylistTrackCommandPayload(string PlaylistId, string PlaylistName, string SongUrl);
