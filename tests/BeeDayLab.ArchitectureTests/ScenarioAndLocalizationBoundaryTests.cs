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

    /// <summary>
    /// Sprint 33.12 (Issue #373) guard: proves none of this Sprint's new Identity &amp; Account
    /// surface files (Login/CreateProfile/Tutorial/Account/etc. and their scenario providers) leak a
    /// real Domain/Application/Infrastructure dependency, and — the boundary specific to this
    /// Sprint's brief — that <c>DomainErrorLocalizer</c> is never called from any of them, since it
    /// is explicitly excluded (never ported, same treatment <c>ValidationMessageLocalizer.cs</c> got
    /// in Sprint 33.8) rather than adapted. A failure page instead carries a synthetic outcome
    /// straight from a scenario provider or SharedResources' own already-real "DomainErrorGeneric"
    /// string — never a translated real exception.
    /// </summary>
    [Fact]
    public void NoIdentityAccountSurfaceFileReferencesTheProductionBackendOrTheExcludedDomainErrorLocalizer()
    {
        var identityDirectory = Path.Combine(FindRepositoryRoot(), "src", "BeeDayLab.Web", "Components", "Pages", "Identity");
        Assert.True(Directory.Exists(identityDirectory), $"Expected directory not found: {identityDirectory}");

        var sourceFiles = Directory.EnumerateFiles(identityDirectory, "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(identityDirectory, "*.razor", SearchOption.AllDirectories))
            .ToList();
        Assert.NotEmpty(sourceFiles);

        var forbidden = ForbiddenSubstrings.Append("DomainErrorLocalizer").ToArray();
        var violations = new List<string>();

        foreach (var file in sourceFiles)
        {
            var content = StripComments(File.ReadAllText(file));

            foreach (var term in forbidden)
            {
                if (content.Contains(term, StringComparison.Ordinal))
                {
                    violations.Add($"{Path.GetFileName(file)} references '{term}'");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "Identity & Account surface boundary violated (Sprint 33.12 brief):" + Environment.NewLine
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

    /// <summary>
    /// Strips <c>//</c>/<c>///</c> line comments, <c>/* */</c> block comments, and (Sprint 33.12
    /// addition, needed once this file started scanning .razor files too) <c>@* *@</c> Razor
    /// comments, so architecture assertions target real code, not explanatory documentation — this
    /// Sprint's own adaptation-note comments legitimately name every excluded dependency they explain
    /// (e.g. "DomainErrorLocalizer.Translate(ex, SharedLocalizer) is replaced with...").
    /// </summary>
    private static string StripComments(string content)
    {
        content = Regex.Replace(content, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
        content = Regex.Replace(content, @"@\*.*?\*@", string.Empty, RegexOptions.Singleline);
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
