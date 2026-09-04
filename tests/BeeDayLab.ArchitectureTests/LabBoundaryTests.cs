using Xunit;

namespace BeeDayLab.ArchitectureTests;

/// <summary>
/// Proves the ADR-008 boundary in code: no project under this solution may reference the BeeDay
/// production backend (Domain/Application/Infrastructure), EF Core, or a SQL Server driver.
/// Runs against the raw .csproj files, not loaded assemblies, so it also catches a reference
/// that was added but never actually used.
/// </summary>
public sealed class LabBoundaryTests
{
    private static readonly string[] ForbiddenReferenceSubstrings =
    [
        "BeeDay.Domain",
        "BeeDay.Application",
        "BeeDay.Infrastructure",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.Data.SqlClient",
        "System.Data.SqlClient",
    ];

    [Fact]
    public void NoProjectFileReferencesTheProductionBackendOrADatabaseDriver()
    {
        var repoRoot = FindRepositoryRoot();
        var csprojFiles = Directory.EnumerateFiles(repoRoot, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                        && !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(csprojFiles);

        var violations = new List<string>();

        foreach (var csprojFile in csprojFiles)
        {
            var content = File.ReadAllText(csprojFile);

            foreach (var forbidden in ForbiddenReferenceSubstrings)
            {
                if (content.Contains(forbidden, StringComparison.OrdinalIgnoreCase))
                {
                    violations.Add($"{Path.GetFileName(csprojFile)} references '{forbidden}'");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "Lab boundary violated (ADR-008):" + Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void WebProjectHasNoSqlOrLocalDbConnectionStringConfigured()
    {
        var repoRoot = FindRepositoryRoot();
        var appSettingsFiles = Directory.EnumerateFiles(repoRoot, "appsettings*.json", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                        && !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(appSettingsFiles);

        foreach (var file in appSettingsFiles)
        {
            var content = File.ReadAllText(file);
            Assert.DoesNotContain("ConnectionStrings", content, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("LocalDB", content, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void NoDuplicateHeroOrBannerComponentExistsBesideBeeDayHero()
    {
        // Sprint 35.1-R3 (OWNER direction change): the OWNER rejected the authenticated full-width
        // Hero direction and moved the authenticated shell to a top-navigation-led composition.
        // WorkspaceHero (EPIC 35 Sprint 35.1-R2's thin structural wrapper around BeeDayHero for the
        // authenticated workspace band) is removed entirely — BeeDayHero (Components/DesignSystem/
        // Layout) is once again the ONLY Hero primitive in the repository, used solely by the public/
        // institutional pages it always served. Guards against a future Sprint introducing an
        // AppBanner/DashboardHero/ProfileHero/WorkspaceHero-style near-duplicate instead of reusing
        // BeeDayHero (public) or BeeDayPageHeader (authenticated, content-first).
        var repoRoot = FindRepositoryRoot();
        var componentsRoot = Path.Combine(repoRoot, "src", "BeeDayLab.Web", "Components");
        var componentFileNames = Directory.EnumerateFiles(componentsRoot, "*.razor", SearchOption.AllDirectories)
            .Select(Path.GetFileName)
            .Select(name => name!)
            .ToList();

        var heroFiles = componentFileNames
            .Where(name => name.EndsWith("Hero.razor", StringComparison.Ordinal))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();
        var bannerFiles = componentFileNames
            .Where(name => name.Contains("Banner", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.Equal(["BeeDayHero.razor"], heroFiles);
        Assert.Empty(bannerFiles);
    }

    [Fact]
    public void NoDuplicateDesktopNavigationSourceExistsBesideNavigationItems()
    {
        // Sprint 35.1-R3: DesktopNavigation (the new horizontal top bar) and MobileSidebar both
        // render the exact same <NavigationItems /> — the one canonical navigation-destination
        // source. Guards against a future Sprint hardcoding a second, divergent list of
        // Profile/Daily/Wallet/Account destinations directly into DesktopNavigation (or anywhere
        // else) instead of reusing NavigationItems, and against the historical TopNavigation
        // component (removed EPIC 21 Sprint 21.3 in production BeeDay) being reintroduced verbatim.
        var repoRoot = FindRepositoryRoot();
        var layoutRoot = Path.Combine(repoRoot, "src", "BeeDayLab.Web", "Components", "Layout");
        var componentFileNames = Directory.EnumerateFiles(layoutRoot, "*.razor", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Select(name => name!)
            .ToList();

        Assert.Contains("NavigationItems.razor", componentFileNames);
        Assert.Contains("DesktopNavigation.razor", componentFileNames);
        Assert.DoesNotContain("DesktopSidebar.razor", componentFileNames);
        Assert.DoesNotContain("TopNavigation.razor", componentFileNames);

        var desktopNavigationMarkup = File.ReadAllText(Path.Combine(layoutRoot, "DesktopNavigation.razor"));
        Assert.Contains("<NavigationItems", desktopNavigationMarkup, StringComparison.Ordinal);

        var mobileSidebarMarkup = File.ReadAllText(Path.Combine(layoutRoot, "MobileSidebar.razor"));
        Assert.Contains("<NavigationItems", mobileSidebarMarkup, StringComparison.Ordinal);
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
