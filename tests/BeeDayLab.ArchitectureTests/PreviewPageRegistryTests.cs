using System.Text.RegularExpressions;
using Xunit;

namespace BeeDayLab.ArchitectureTests;

/// <summary>
/// Sprint 33.17 (Issue #378) "Every extracted page/e-mail is reachable" acceptance criterion,
/// enforced in code rather than by convention: cross-checks
/// <c>BeeDayLab.Web.Components.Pages.Preview.PreviewPageRegistry</c> against every real
/// <c>@page "..."</c> directive declared under <c>Components/Pages/</c> — in both directions, so
/// the registry can neither miss a real route nor list one that no longer exists. Text-based
/// source scanning, like every other guard in this project (<see cref="LabBoundaryTests"/>,
/// <see cref="ScenarioAndLocalizationBoundaryTests"/>) — this project deliberately carries no
/// <c>ProjectReference</c> to <c>BeeDayLab.Web</c>, so the registry is read as source text, not a
/// compiled type.
/// </summary>
public sealed class PreviewPageRegistryTests
{
    private static readonly Regex PageDirective = new("@page\\s+\"([^\"]+)\"", RegexOptions.Compiled);
    private static readonly Regex RegistryEntry = new("new\\(\"([^\"]+)\"", RegexOptions.Compiled);

    // Lab-only utility/gallery routes that are not themselves individually listed as index entries
    // (the Gallery/Preview pages are the surfaces doing the listing, not additional listed pages;
    // /emails is represented in the registry via its 2 deep-linked ?template= variants instead of
    // its own bare route).
    private static readonly string[] ExcludedFromRegistryCoverage = ["/design-system", "/preview", "/emails"];

    [Fact]
    public void EveryDeclaredPageRouteIsListedInTheRegistry()
    {
        var declaredRoutes = ReadDeclaredRoutes().Except(ExcludedFromRegistryCoverage).ToArray();
        Assert.NotEmpty(declaredRoutes);

        var registryPaths = ReadRegistryPaths().Select(path => path.Split('?')[0]).ToHashSet(StringComparer.Ordinal);

        var missing = declaredRoutes.Where(route => !registryPaths.Contains(route)).ToArray();

        Assert.True(
            missing.Length == 0,
            "Route(s) declared via @page but missing from PreviewPageRegistry: " + string.Join(", ", missing));
    }

    [Fact]
    public void EveryRegistryEntryPointsAtARealDeclaredRoute()
    {
        var declaredRoutes = ReadDeclaredRoutes().ToHashSet(StringComparer.Ordinal);

        var stale = ReadRegistryPaths()
            .Select(path => path.Split('?')[0])
            .Where(path => !declaredRoutes.Contains(path))
            .ToArray();

        Assert.True(
            stale.Length == 0,
            "PreviewPageRegistry entry/entries point at a route no @page directive declares: " + string.Join(", ", stale));
    }

    [Fact]
    public void TheRegistryHasNoDuplicateEntries()
    {
        var duplicates = ReadRegistryPaths()
            .GroupBy(path => path, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        Assert.True(duplicates.Length == 0, "PreviewPageRegistry has duplicate path(s): " + string.Join(", ", duplicates));
    }

    private static IEnumerable<string> ReadDeclaredRoutes()
    {
        var pagesDirectory = Path.Combine(FindRepositoryRoot(), "src", "BeeDayLab.Web", "Components", "Pages");
        return Directory.EnumerateFiles(pagesDirectory, "*.razor", SearchOption.AllDirectories)
            .SelectMany(file => PageDirective.Matches(File.ReadAllText(file)).Select(m => m.Groups[1].Value))
            .Distinct();
    }

    private static IEnumerable<string> ReadRegistryPaths()
    {
        var registryPath = Path.Combine(
            FindRepositoryRoot(), "src", "BeeDayLab.Web", "Components", "Pages", "Preview", "PreviewPageRegistry.cs");
        Assert.True(File.Exists(registryPath), $"Expected file not found: {registryPath}");

        var content = File.ReadAllText(registryPath);
        return RegistryEntry.Matches(content).Select(m => m.Groups[1].Value);
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
