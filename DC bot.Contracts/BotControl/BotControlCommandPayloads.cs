namespace DC_bot.BotControl;

public sealed record QueueEnqueueCommandPayload(
    string Query,
    string? SearchMode,
    string? RequestedBy,
    ulong? VoiceChannelId = null,
    ulong? TextChannelId = null);

public sealed record QueueRemoveCommandPayload(int TrackNumber);

public sealed record QueueMoveCommandPayload(int TrackIndex);

public sealed record PlaylistCommandPayload(string PlaylistId, string PlaylistName);

public sealed record PlaylistTrackCommandPayload(string PlaylistId, string PlaylistName, string SongUrl);

public sealed record PlaylistImportCommandPayload(string Name, string Url);
