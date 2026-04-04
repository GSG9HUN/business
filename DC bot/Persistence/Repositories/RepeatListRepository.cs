using DC_bot.Interface.Service.Persistence;
using DC_bot.Persistence.Db;
using DC_bot.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace DC_bot.Persistence.Repositories;

public class RepeatListRepository(IDbContextFactory<BotDbContext> dbContextFactory) : IRepeatListRepository
{
	private const int MaxRepeatListItemsPerGuild = 100;

	public async Task<IReadOnlyList<string>> GetTrackIdentifiersAsync(
		ulong guildId,
		CancellationToken cancellationToken = default)
	{
		await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
		var id = ToDbGuildId(guildId);

		return await dbContext.GuildRepeatListItems
			.AsNoTracking()
			.Where(item => item.GuildId == id)
			.OrderBy(item => item.Position)
			.Select(item => item.TrackIdentifier)
			.ToListAsync(cancellationToken);
	}

	public async Task ReplaceAsync(
		ulong guildId,
		IReadOnlyList<string> trackIdentifiers,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(trackIdentifiers);

		if (trackIdentifiers.Count > MaxRepeatListItemsPerGuild)
		{
			throw new InvalidOperationException($"Repeat list cannot contain more than {MaxRepeatListItemsPerGuild} tracks.");
		}

		await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
		var id = ToDbGuildId(guildId);

		await EnsureGuildDataExistsAsync(dbContext, id, cancellationToken);

		var existingItems = await dbContext.GuildRepeatListItems
			.Where(item => item.GuildId == id)
			.ToListAsync(cancellationToken);

		await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

		dbContext.GuildRepeatListItems.RemoveRange(existingItems);
		await dbContext.SaveChangesAsync(cancellationToken);

		if (trackIdentifiers.Count > 0)
		{
			var now = DateTimeOffset.UtcNow;
			var newItems = trackIdentifiers
				.Select((trackIdentifier, index) => new GuildRepeatListItemEntity
				{
					GuildId = id,
					Position = index,
					TrackIdentifier = trackIdentifier,
					AddedAtUtc = now
				})
				.ToList();

			dbContext.GuildRepeatListItems.AddRange(newItems);
			await dbContext.SaveChangesAsync(cancellationToken);
		}

		await transaction.CommitAsync(cancellationToken);
	}

	public async Task ClearAsync(ulong guildId, CancellationToken cancellationToken = default)
	{
		await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
		var id = ToDbGuildId(guildId);

		var existingItems = await dbContext.GuildRepeatListItems
			.Where(item => item.GuildId == id)
			.ToListAsync(cancellationToken);

		if (existingItems.Count == 0)
		{
			return;
		}

		dbContext.GuildRepeatListItems.RemoveRange(existingItems);
		await dbContext.SaveChangesAsync(cancellationToken);
	}

	private static async Task EnsureGuildDataExistsAsync(
		BotDbContext dbContext,
		long guildId,
		CancellationToken cancellationToken)
	{
		var exists = await dbContext.GuildData.AnyAsync(g => g.GuildId == guildId, cancellationToken);
		if (exists)
		{
			return;
		}

		dbContext.GuildData.Add(new GuildDataEntity
		{
			GuildId = guildId,
			IsPremium = false,
			UpdatedAtUtc = DateTimeOffset.UtcNow
		});

		await dbContext.SaveChangesAsync(cancellationToken);
	}

	private static long ToDbGuildId(ulong guildId)
	{
		return checked((long)guildId);
	}

}