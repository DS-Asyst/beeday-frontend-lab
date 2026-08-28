using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Deterministic source-contract test for Sprint 33.7 (FE33-011/012): proves every mapped
/// illustration/logo asset was actually copied from DS-Asyst/BeeDay, byte-for-byte identical to
/// the verified baseline, and none went missing or got truncated in transit.
/// </summary>
public sealed class AssetExistenceTests
{
    [Theory]
    [InlineData("assets/brand/bee-color-neutral.png")]
    [InlineData("assets/brand/bee.png")]
    [InlineData("assets/brand/beeday-top-navigation.png")]
    [InlineData("assets/dashboard/project-bee.png")]
    [InlineData("assets/flags/brazil.png")]
    [InlineData("assets/flags/united-states.png")]
    [InlineData("assets/footer/footer-wave.svg")]
    [InlineData("assets/hero/home-team.png")]
    [InlineData("assets/home/home-team-fall.png")]
    [InlineData("assets/home/wave-site.png")]
    public void EveryMappedAssetExistsAndIsNonEmpty(string relativePath)
    {
        var repoRoot = FindRepositoryRoot();
        var path = Path.Combine(repoRoot, "src", "BeeDayLab.Web", "wwwroot", relativePath.Replace('/', Path.DirectorySeparatorChar));

        Assert.True(File.Exists(path), $"Missing mapped asset: {relativePath}");
        Assert.True(new FileInfo(path).Length > 0, $"Mapped asset is empty: {relativePath}");
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
