using System.Xml.Linq;

namespace DC_bot_tests.UnitTests.Configuration;

[Trait("Category", "Unit")]
public class ProductionProjectDependencyTests
{
    private static readonly string[] ForbiddenProductionPackages =
    [
        "coverlet.collector",
        "Microsoft.NET.Test.Sdk",
        "Moq",
        "Testcontainers",
        "Testcontainers.PostgreSql",
        "xunit",
        "xunit.runner.visualstudio"
    ];

    [Fact]
    public void ProductionProject_ShouldNotReferenceTestOnlyPackages()
    {
        var projectPath = Path.Combine(FindRepositoryRoot(), "DC bot", "DC bot.csproj");
        var document = XDocument.Load(projectPath);
        var packages = document
            .Descendants()
            .Where(element => element.Name.LocalName == "PackageReference")
            .Select(element =>
                (string?)element.Attribute("Include") ??
                (string?)element.Attribute("Update"))
            .Where(package => !string.IsNullOrWhiteSpace(package))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var violations = ForbiddenProductionPackages
            .Where(package => packages.Contains(package))
            .OrderBy(package => package)
            .ToArray();

        Assert.Empty(violations);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "DC bot", "DC bot.csproj")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root containing 'DC bot/DC bot.csproj'.");
    }
}