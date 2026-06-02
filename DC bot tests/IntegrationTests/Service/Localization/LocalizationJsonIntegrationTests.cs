using DC_bot.Constants;
using DC_bot.Interface.Service.IO;
using DC_bot.Service;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot_tests.IntegrationTests.Service.Localization;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class LocalizationJsonIntegrationTests
{
    [Fact]
    public void LocalizationService_WithRealJsonFiles_LoadsEnglishAndHungarianSlashFallbacks()
    {
        using var fileSystem = new RepositoryLocalizationFileSystem();
        var service = new LocalizationService(NullLogger<LocalizationService>.Instance, fileSystem);
        const ulong hungarianGuildId = 9401UL;

        Assert.Equal("The language changed successfully.", service.Get(LocalizationKeys.LanguageCommandResponse));
        Assert.Equal("This command can only be used in a server.", service.Get(LocalizationKeys.SlashCommandGuildOnly));
        Assert.Equal("Request accepted.", service.Get(LocalizationKeys.SlashCommandDeferredAccepted));
        Assert.Equal("Command 'play' is not registered.", service.Get(LocalizationKeys.SlashCommandNotRegistered, "play"));
        Assert.Equal(
            "An unexpected error occurred while executing the command.",
            service.Get(LocalizationKeys.SlashCommandUnexpectedError));

        service.SaveLanguage(hungarianGuildId, "hu");

        Assert.Contains("nyelv", service.Get(hungarianGuildId, LocalizationKeys.LanguageCommandResponse));
        Assert.Contains("szerveren", service.Get(hungarianGuildId, LocalizationKeys.SlashCommandGuildOnly));
        Assert.Contains("elfogadva", service.Get(hungarianGuildId, LocalizationKeys.SlashCommandDeferredAccepted));
        Assert.Contains("play", service.Get(hungarianGuildId, LocalizationKeys.SlashCommandNotRegistered, "play"));
        Assert.Contains("hiba", service.Get(hungarianGuildId, LocalizationKeys.SlashCommandUnexpectedError));
    }

    private sealed class RepositoryLocalizationFileSystem : IFileSystem, IDisposable
    {
        private readonly string _guildLocalizationDirectory = Path.Combine(
            Path.GetTempPath(),
            $"dc-bot-localization-{Guid.NewGuid():N}");

        private readonly string _repositoryLocalizationDirectory = Path.Combine(
            FindRepositoryRoot(),
            "DC bot",
            "localization");

        public bool DirectoryExists(string path)
        {
            return IsGuildLocalizationPath(path)
                ? Directory.Exists(_guildLocalizationDirectory)
                : Directory.Exists(path);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(IsGuildLocalizationPath(path) ? _guildLocalizationDirectory : path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(MapPath(path));
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(MapPath(path));
        }

        public void WriteAllText(string path, string contents)
        {
            var mappedPath = MapPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(mappedPath)!);
            File.WriteAllText(mappedPath, contents);
        }

        public void Dispose()
        {
            if (Directory.Exists(_guildLocalizationDirectory))
            {
                Directory.Delete(_guildLocalizationDirectory, recursive: true);
            }
        }

        private string MapPath(string path)
        {
            if (IsGuildLocalizationPath(path))
            {
                return Path.Combine(_guildLocalizationDirectory, Path.GetFileName(path));
            }

            if (Path.GetFileName(path) is "eng.json" or "hu.json")
            {
                return Path.Combine(_repositoryLocalizationDirectory, Path.GetFileName(path));
            }

            return path;
        }

        private static bool IsGuildLocalizationPath(string path)
        {
            return path.Contains("guildFiles", StringComparison.OrdinalIgnoreCase) &&
                   path.Contains("localization", StringComparison.OrdinalIgnoreCase);
        }

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, "DC bot", "localization")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Could not locate repository root containing DC bot/localization.");
        }
    }
}
