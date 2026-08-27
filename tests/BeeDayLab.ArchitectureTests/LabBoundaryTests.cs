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
