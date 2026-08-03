namespace DC_bot.Interface.Service.Persistence;

public interface IRepeatListRepository
{
	Task<IReadOnlyList<string>> GetTrackIdentifiersAsync(
		ulong guildId,
		CancellationToken cancellationToken = default);

	Task ReplaceAsync(
		ulong guildId,
		IReadOnlyList<string> trackIdentifiers,
		CancellationToken cancellationToken = default);

	Task ClearAsync(ulong guildId, CancellationToken cancellationToken = default);

}