using DC_bot.Db;
using DC_bot.Entities.MobileApps;
using DC_bot.Entities.MobileAppUserSettings;
using DC_bot.Interface.Service.Persistence.Models.MobileAppUserSettings;
using DC_bot.Repositories.MobileAppUserSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.IntegrationTests.Persistence;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class MobileAppUserSettingsRepositoryPostgreSqlIntegrationTests
{
    [Fact]
    public async Task GetOrCreateAsync_WithMigratedDatabase_CreatesDefaultSettings()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();
        var factory = services.GetRequiredService<IDbContextFactory<BotDbContext>>();
        var repository = new MobileAppUserSettingsRepository(factory);

        await SeedMobileAppUserAsync(factory, 1000ul);

        var settings = await repository.GetOrCreateAsync(1000ul);

        Assert.Equal(1000ul, settings.DiscordUserId);
        Assert.Equal(MobileAppLanguageCode.English, settings.LanguageCode);
        Assert.Equal(MobileAppTheme.System, settings.Theme);
        Assert.True(settings.HapticFeedbackEnabled);
        Assert.True(settings.SoundEffectsEnabled);
        Assert.False(settings.TelemetryEnabled);

        await using var dbContext = await factory.CreateDbContextAsync();
        Assert.Single(dbContext.MobileAppUserSettings);
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenSettingsAlreadyExist_ReadsExistingSettings()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();
        var factory = services.GetRequiredService<IDbContextFactory<BotDbContext>>();
        var repository = new MobileAppUserSettingsRepository(factory);

        await SeedMobileAppUserAsync(factory, 2000ul);
        await SeedMobileAppUserSettingsAsync(
            factory,
            2000ul,
            MobileAppLanguageCode.Hungarian,
            MobileAppTheme.Dark,
            hapticFeedbackEnabled: false,
            soundEffectsEnabled: false,
            telemetryEnabled: true);

        var settings = await repository.GetOrCreateAsync(2000ul);

        Assert.Equal(MobileAppLanguageCode.Hungarian, settings.LanguageCode);
        Assert.Equal(MobileAppTheme.Dark, settings.Theme);
        Assert.False(settings.HapticFeedbackEnabled);
        Assert.False(settings.SoundEffectsEnabled);
        Assert.True(settings.TelemetryEnabled);
    }

    [Fact]
    public async Task PatchAsync_WithMigratedDatabase_UpdatesOnlyProvidedFields()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();
        var factory = services.GetRequiredService<IDbContextFactory<BotDbContext>>();
        var repository = new MobileAppUserSettingsRepository(factory);

        await SeedMobileAppUserAsync(factory, 3000ul);
        await repository.GetOrCreateAsync(3000ul);

        var settings = await repository.PatchAsync(
            3000ul,
            new MobileAppUserSettingsPatchRecord(
                LanguageCode: null,
                Theme: MobileAppTheme.Light,
                HapticFeedbackEnabled: false,
                SoundEffectsEnabled: null,
                TelemetryEnabled: true));

        Assert.Equal(MobileAppLanguageCode.English, settings.LanguageCode);
        Assert.Equal(MobileAppTheme.Light, settings.Theme);
        Assert.False(settings.HapticFeedbackEnabled);
        Assert.True(settings.SoundEffectsEnabled);
        Assert.True(settings.TelemetryEnabled);
    }

    [Theory]
    [InlineData("eng", "amoled")]
    [InlineData("toolongcode1", "system")]
    public async Task MobileAppUserSettings_WithInvalidPersistedValues_FailsDatabaseConstraints(
        string languageCode,
        string theme)
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();
        var factory = services.GetRequiredService<IDbContextFactory<BotDbContext>>();

        await SeedMobileAppUserAsync(factory, 4000ul);

        await using var dbContext = await factory.CreateDbContextAsync();
        dbContext.MobileAppUserSettings.Add(new MobileAppUserSettingsEntity
        {
            DiscordUserId = 4000ul,
            LanguageCode = languageCode,
            Theme = theme,
            HapticFeedbackEnabled = true,
            SoundEffectsEnabled = true,
            TelemetryEnabled = false,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task MobileAppUserSettings_WhenUserDoesNotExist_FailsForeignKeyConstraint()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();
        var factory = services.GetRequiredService<IDbContextFactory<BotDbContext>>();

        await using var dbContext = await factory.CreateDbContextAsync();
        dbContext.MobileAppUserSettings.Add(new MobileAppUserSettingsEntity
        {
            DiscordUserId = 5000ul,
            LanguageCode = MobileAppLanguageCode.English,
            Theme = MobileAppTheme.System,
            HapticFeedbackEnabled = true,
            SoundEffectsEnabled = true,
            TelemetryEnabled = false,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenCalledConcurrently_CreatesSingleSettingsRow()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();
        var factory = services.GetRequiredService<IDbContextFactory<BotDbContext>>();
        const ulong discordUserId = 6000ul;

        await SeedMobileAppUserAsync(factory, discordUserId);

        var settings = await Task.WhenAll(Enumerable.Range(0, 5)
            .Select(_ => new MobileAppUserSettingsRepository(factory).GetOrCreateAsync(discordUserId)));

        Assert.All(settings, item =>
        {
            Assert.Equal(discordUserId, item.DiscordUserId);
            Assert.Equal(MobileAppLanguageCode.English, item.LanguageCode);
            Assert.Equal(MobileAppTheme.System, item.Theme);
        });

        await using var dbContext = await factory.CreateDbContextAsync();
        Assert.Single(await dbContext.MobileAppUserSettings
            .Where(item => item.DiscordUserId == discordUserId)
            .ToListAsync());
    }

    private static async Task SeedMobileAppUserAsync(
        IDbContextFactory<BotDbContext> factory,
        ulong discordUserId)
    {
        await using var dbContext = await factory.CreateDbContextAsync();
        dbContext.MobileAppUsers.Add(new MobileAppUserEntity
        {
            DiscordUserId = discordUserId,
            Username = $"user-{discordUserId}",
            GlobalName = null,
            AvatarHash = null,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            LastLoginAtUtc = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedMobileAppUserSettingsAsync(
        IDbContextFactory<BotDbContext> factory,
        ulong discordUserId,
        string languageCode,
        string theme,
        bool hapticFeedbackEnabled,
        bool soundEffectsEnabled,
        bool telemetryEnabled)
    {
        await using var dbContext = await factory.CreateDbContextAsync();
        dbContext.MobileAppUserSettings.Add(new MobileAppUserSettingsEntity
        {
            DiscordUserId = discordUserId,
            LanguageCode = languageCode,
            Theme = theme,
            HapticFeedbackEnabled = hapticFeedbackEnabled,
            SoundEffectsEnabled = soundEffectsEnabled,
            TelemetryEnabled = telemetryEnabled,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();
    }
}
