using BeeDayLab.Web.Components.DesignSystem.Icons;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Deterministic source-contract test for Sprint 33.7 (FE33-009/010): proves the icon registry
/// copied from DS-Asyst/BeeDay resolves every BeeDayIconName and that every resolved symbol id
/// actually exists in the copied sprite.svg — catching a registry/sprite drift the C# compiler
/// alone cannot.
/// </summary>
public sealed class IconSystemParityTests
{
    private static string SpriteSvgContent
    {
        get
        {
            var repoRoot = FindRepositoryRoot();
            var path = Path.Combine(repoRoot, "src", "BeeDayLab.Web", "wwwroot", "icons", "sprite.svg");
            return File.ReadAllText(path);
        }
    }

    [Fact]
    public void RegistryHasExactlyOneDefinitionPerEnumValue()
    {
        var enumValues = Enum.GetValues<BeeDayIconName>();

        Assert.Equal(enumValues.Length, BeeDayIconRegistry.All.Count);

        foreach (var name in enumValues)
        {
            Assert.True(BeeDayIconRegistry.TryGet(name, out _), $"No registry definition for {name}");
        }
    }

    [Theory]
    [MemberData(nameof(AllIconNames))]
    public void EveryRegisteredSymbolIdExistsInTheSprite(BeeDayIconName name)
    {
        var definition = BeeDayIconRegistry.Resolve(name);

        Assert.Contains($"<symbol id=\"{definition.SymbolId}\"", SpriteSvgContent, StringComparison.Ordinal);
    }

    [Fact]
    public void UnresolvedNameFallsBackToWarning()
    {
        var fallback = BeeDayIconRegistry.Resolve((BeeDayIconName)9999);

        Assert.Equal(BeeDayIconRegistry.Resolve(BeeDayIconName.Warning), fallback);
    }

    [Theory]
    [InlineData(BeeDayIconName.Add, "add", BeeDayIconCategory.Actions)]
    [InlineData(BeeDayIconName.Search, "search", BeeDayIconCategory.Actions)]
    [InlineData(BeeDayIconName.Warning, "warning", BeeDayIconCategory.Feedback)]
    [InlineData(BeeDayIconName.GitHub, "github", BeeDayIconCategory.Social)]
    [InlineData(BeeDayIconName.LinkedIn, "linkedin", BeeDayIconCategory.Social)]
    [InlineData(BeeDayIconName.Wallet, "wallet", BeeDayIconCategory.Statistics)]
    public void SpotCheckedDefinitionsMatchTheVerifiedBaseline(
        BeeDayIconName name,
        string expectedSymbolId,
        BeeDayIconCategory expectedCategory)
    {
        var definition = BeeDayIconRegistry.Resolve(name);

        Assert.Equal(expectedSymbolId, definition.SymbolId);
        Assert.Equal(expectedCategory, definition.Category);
    }

    public static IEnumerable<object[]> AllIconNames() =>
        Enum.GetValues<BeeDayIconName>().Select(name => new object[] { name });

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
