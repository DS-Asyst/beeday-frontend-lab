using System.Text.RegularExpressions;
using Xunit;

namespace BeeDayLab.ArchitectureTests;

/// <summary>
/// Sprint 33.10 (FE33-104, Issue #371) architecture guards, in the same source-text-scanning style
/// as <see cref="LabBoundaryTests"/>: proves the scenario engine under
/// <c>BeeDayLab.Web.Scenarios</c> has no service/database coupling (Issue #371's acceptance
/// criterion, enforced in code, not just by convention), and that the Lab's culture catalog
/// (<c>LabCultures.cs</c>) carries no Domain dependency — the reason its production counterpart's
/// two conversion methods (<c>FromUserLanguage</c>/<c>ToUserLanguage</c>) were deliberately dropped
/// rather than ported.
/// </summary>
public sealed class ScenarioAndLocalizationBoundaryTests
{
    private static readonly string[] ForbiddenSubstrings =
    [
        "BeeDay.Domain",
        "BeeDay.Application",
        "BeeDay.Infrastructure",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.Data.SqlClient",
        "System.Data.SqlClient",
        "ISender",
        "BeeDayWebService",
        "ConnectionString",
        "HttpClient",
        "System.Net.Http",
    ];

    [Fact]
    public void NoScenarioEngineFileReferencesAServiceOrDatabase()
    {
        var scenariosDirectory = Path.Combine(FindRepositoryRoot(), "src", "BeeDayLab.Web", "Scenarios");
        Assert.True(Directory.Exists(scenariosDirectory), $"Expected directory not found: {scenariosDirectory}");

        var sourceFiles = Directory.EnumerateFiles(scenariosDirectory, "*.cs", SearchOption.AllDirectories).ToList();
        Assert.NotEmpty(sourceFiles);

        var violations = new List<string>();

        foreach (var file in sourceFiles)
        {
            var content = StripComments(File.ReadAllText(file));

            foreach (var forbidden in ForbiddenSubstrings)
            {
                if (content.Contains(forbidden, StringComparison.Ordinal))
                {
                    violations.Add($"{Path.GetFileName(file)} references '{forbidden}'");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "Scenario engine boundary violated (Issue #371 acceptance criterion):" + Environment.NewLine
                + string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void ScenarioEngineUsesOnlyDeterministicPrimitivesNoRandomOrWallClock()
    {
        var scenariosDirectory = Path.Combine(FindRepositoryRoot(), "src", "BeeDayLab.Web", "Scenarios");
        var forbiddenDeterminismSubstrings = new[]
        {
            "new Random(",
            "Guid.NewGuid()",
            "DateTime.Now",
            "DateTimeOffset.UtcNow",
            "DateTime.UtcNow",
        };

        var violations = new List<string>();

        foreach (var file in Directory.EnumerateFiles(scenariosDirectory, "*.cs", SearchOption.AllDirectories))
        {
            var content = StripComments(File.ReadAllText(file));

            foreach (var forbidden in forbiddenDeterminismSubstrings)
            {
                if (content.Contains(forbidden, StringComparison.Ordinal))
                {
                    violations.Add($"{Path.GetFileName(file)} uses non-deterministic '{forbidden}'");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "Scenario providers must be pure functions of ScenarioContext:" + Environment.NewLine
                + string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void LabCulturesHasNoDomainDependency()
    {
        var labCulturesPath = Path.Combine(
            FindRepositoryRoot(), "src", "BeeDayLab.Web", "Localization", "LabCultures.cs");
        Assert.True(File.Exists(labCulturesPath), $"Expected file not found: {labCulturesPath}");

        // Only the code itself must be Domain-free — its XML doc comments legitimately name
        // BeeDay.Domain.Enums.UserLanguage/FromUserLanguage/ToUserLanguage to explain why those
        // conversion methods were deliberately dropped rather than ported, so comments are
        // stripped before asserting.
        var content = StripComments(File.ReadAllText(labCulturesPath));

        Assert.DoesNotContain("BeeDay.Domain", content, StringComparison.Ordinal);
        Assert.DoesNotContain("UserLanguage", content, StringComparison.Ordinal);
        Assert.DoesNotContain("FromUserLanguage", content, StringComparison.Ordinal);
        Assert.DoesNotContain("ToUserLanguage", content, StringComparison.Ordinal);
    }

    [Fact]
    public void NoAuthenticatedAccountCultureProviderIsPortedIntoTheLab()
    {
        // AuthenticatedAccountCultureProvider is 100% real backend infrastructure (reads a real
        // User entity handed off from real authentication middleware) — EXCLUDE, full stop. This
        // guards against a future Sprint accidentally reintroducing it. Comments legitimately name
        // it (explaining the exclusion), so they are stripped before asserting on actual code.
        var localizationDirectory = Path.Combine(FindRepositoryRoot(), "src", "BeeDayLab.Web", "Localization");
        Assert.True(Directory.Exists(localizationDirectory));

        foreach (var file in Directory.EnumerateFiles(localizationDirectory, "*.cs", SearchOption.AllDirectories))
        {
            Assert.DoesNotContain(
                "AuthenticatedAccountCultureProvider",
                StripComments(File.ReadAllText(file)),
                StringComparison.Ordinal);
        }
    }

    /// <summary>Strips <c>//</c>/<c>///</c> line comments and <c>/* */</c> block comments so architecture assertions target real code, not explanatory documentation.</summary>
    private static string StripComments(string content)
    {
        content = Regex.Replace(content, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
        content = Regex.Replace(content, @"//.*$", string.Empty, RegexOptions.Multiline);
        return content;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BeeDayLab.slnx")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory!.FullName;
    }
}
