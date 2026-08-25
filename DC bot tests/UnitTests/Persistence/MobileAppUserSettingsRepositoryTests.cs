using DC_bot.Entities.MobileAppUserSettings;
using DC_bot.Interface.Service.Persistence.Models.MobileAppUserSettings;
using DC_bot.Repositories;
using DC_bot.Repositories.MobileAppUserSettings;

namespace DC_bot_tests.UnitTests.Persistence;

[Trait("Category", "Unit")]
public class MobileAppUserSettingsRepositoryTests
{
    private static InMemoryDbContextFactory CreateFactory() =>
        new($"MobileAppUserSettings_{Guid.NewGuid()}");

    [Fact]
    public async Task GetOrCreateAsync_WhenNoSettingsExist_CreatesDefaultSettings()
    {
        var factory = CreateFactory();
        var repository = new MobileAppUserSettingsRepository(factory);

        var settings = await repository.GetOrCreateAsync(100ul);

        Assert.Equal(100ul, settings.DiscordUserId);
        Assert.Equal(MobileAppLanguageCode.English, settings.LanguageCode);
        Assert.Equal(MobileAppTheme.System, settings.Theme);
        Assert.True(settings.HapticFeedbackEnabled);
        Assert.True(settings.SoundEffectsEnabled);
        Assert.False(settings.TelemetryEnabled);

        await using var dbContext = factory.CreateDbContext();
        Assert.Single(dbContext.MobileAppUserSettings);
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenSettingsAlreadyExist_ReturnsExistingSettings()
    {
        var factory = CreateFactory();
        var repository = new MobileAppUserSettingsRepository(factory);

        await using (var dbContext = factory.CreateDbContext())
        {
            dbContext.MobileAppUserSettings.Add(new MobileAppUserSettingsEntity
            {
                DiscordUserId = 200ul,
                LanguageCode = MobileAppLanguageCode.Hungarian,
                Theme = MobileAppTheme.Dark,
                HapticFeedbackEnabled = false,
                SoundEffectsEnabled = false,
                TelemetryEnabled = true,
                UpdatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1)
            });
            await dbContext.SaveChangesAsync();
        }

        var settings = await repository.GetOrCreateAsync(200ul);

        Assert.Equal(MobileAppLanguageCode.Hungarian, settings.LanguageCode);
        Assert.Equal(MobileAppTheme.Dark, settings.Theme);
        Assert.False(settings.HapticFeedbackEnabled);
        Assert.False(settings.SoundEffectsEnabled);
        Assert.True(settings.TelemetryEnabled);

        await using var verifyDbContext = factory.CreateDbContext();
        Assert.Single(verifyDbContext.MobileAppUserSettings);
    }

    [Fact]
    public async Task EnsureExistsAsync_CalledTwice_DoesNotCreateDuplicateSettings()
    {
        var factory = CreateFactory();
        var repository = new MobileAppUserSettingsRepository(factory);

        await repository.EnsureExistsAsync(300ul);
        await repository.EnsureExistsAsync(300ul);

        await using var dbContext = factory.CreateDbContext();
        Assert.Single(dbContext.MobileAppUserSettings);
    }

    [Fact]
    public async Task PatchAsync_WhenPatchHasNoChanges_ReturnsExistingSettings()
    {
        var repository = new MobileAppUserSettingsRepository(CreateFactory());

        await repository.GetOrCreateAsync(400ul);
        var settings = await repository.PatchAsync(
            400ul,
            new MobileAppUserSettingsPatchRecord(
                LanguageCode: null,
                Theme: null,
                HapticFeedbackEnabled: null,
                SoundEffectsEnabled: null,
                TelemetryEnabled: null));

        Assert.Equal(MobileAppLanguageCode.English, settings.LanguageCode);
        Assert.Equal(MobileAppTheme.System, settings.Theme);
        Assert.True(settings.HapticFeedbackEnabled);
        Assert.True(settings.SoundEffectsEnabled);
        Assert.False(settings.TelemetryEnabled);
    }
}
