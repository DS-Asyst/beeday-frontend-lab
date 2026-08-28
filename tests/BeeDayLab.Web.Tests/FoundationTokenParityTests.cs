using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Deterministic source-contract test for Sprint 33.6 (FE33-001..008): proves the Foundation
/// tokens copied from DS-Asyst/BeeDay's variables.css (baseline acce26a) are present with their
/// exact values in the Lab, and stay that way — this is not a live cross-repo diff (ADR-008 rules
/// that out), it is a fixed snapshot contract against the values verified at extraction time.
/// </summary>
public sealed class FoundationTokenParityTests
{
    private static string VariablesCssContent
    {
        get
        {
            var repoRoot = FindRepositoryRoot();
            var path = Path.Combine(repoRoot, "src", "BeeDayLab.Web", "wwwroot", "css", "variables.css");
            return File.ReadAllText(path);
        }
    }

    [Theory]
    [InlineData("--beeday-color-brand-primary: #5247f9;")]
    [InlineData("--beeday-color-brand-primary-hover: #3f33f1;")]
    [InlineData("--beeday-color-brand-primary-active: #1c0ef2;")]
    [InlineData("--beeday-color-brand-primary-light: #827afc;")]
    [InlineData("--beeday-color-brand-primary-soft: #f8f7ff;")]
    [InlineData("--beeday-color-success: #287d4d;")]
    [InlineData("--beeday-color-warning: #d89b22;")]
    [InlineData("--beeday-color-danger: #d33b46;")]
    [InlineData("--beeday-color-info: #335f71;")]
    public void BrandAndSemanticColorTokensMatchTheVerifiedBaseline(string expectedDeclaration)
    {
        Assert.Contains(expectedDeclaration, VariablesCssContent, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("--beeday-spacing-2xs: .125rem;")]
    [InlineData("--beeday-spacing-xs: .25rem;")]
    [InlineData("--beeday-spacing-sm: .5rem;")]
    [InlineData("--beeday-spacing-smd: .75rem;")]
    [InlineData("--beeday-spacing-md: 1rem;")]
    [InlineData("--beeday-spacing-lg: 1.5rem;")]
    [InlineData("--beeday-spacing-xl: 2rem;")]
    [InlineData("--beeday-spacing-2xl: 3rem;")]
    [InlineData("--beeday-spacing-3xl: 4rem;")]
    public void SpacingScaleHasAllNineSteps(string expectedDeclaration)
    {
        Assert.Contains(expectedDeclaration, VariablesCssContent, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("--beeday-radius-xs: .2rem;")]
    [InlineData("--beeday-radius-sm: .375rem;")]
    [InlineData("--beeday-radius-md: .625rem;")]
    [InlineData("--beeday-radius-lg: .75rem;")]
    [InlineData("--beeday-radius-xl: 1rem;")]
    [InlineData("--beeday-radius-2xl: 1.5rem;")]
    [InlineData("--beeday-radius-pill: 999px;")]
    public void RadiusScaleHasAllSevenSteps(string expectedDeclaration)
    {
        Assert.Contains(expectedDeclaration, VariablesCssContent, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("--beeday-shadow-xs: 0 1px 2px rgb(82 71 249 / 6%);")]
    [InlineData("--beeday-shadow-sm: 0 2px 6px rgb(82 71 249 / 8%);")]
    [InlineData("--beeday-shadow-md: 0 4px 12px rgb(82 71 249 / 10%);")]
    [InlineData("--beeday-shadow-lg: 0 12px 28px rgb(82 71 249 / 16%);")]
    public void ShadowScaleHasAllFourSteps(string expectedDeclaration)
    {
        Assert.Contains(expectedDeclaration, VariablesCssContent, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("--beeday-duration-fast: 120ms;")]
    [InlineData("--beeday-duration-normal: 180ms;")]
    [InlineData("--beeday-duration-slow: 260ms;")]
    [InlineData("--beeday-easing-standard: cubic-bezier(.2, 0, 0, 1);")]
    [InlineData("--beeday-easing-emphasized: cubic-bezier(.2, .8, .2, 1);")]
    public void MotionTokensMatchTheVerifiedBaseline(string expectedDeclaration)
    {
        Assert.Contains(expectedDeclaration, VariablesCssContent, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("--beeday-z-navigation: 100;")]
    [InlineData("--beeday-z-drawer-backdrop: 140;")]
    [InlineData("--beeday-z-drawer: 150;")]
    [InlineData("--beeday-z-dropdown: 300;")]
    [InlineData("--beeday-z-modal: 900;")]
    [InlineData("--beeday-z-modal-raised: 1200;")]
    [InlineData("--beeday-z-confirmation: 1400;")]
    [InlineData("--beeday-z-loading: 1500;")]
    [InlineData("--beeday-z-toast: 1700;")]
    public void ZIndexLayersMatchTheVerifiedBaseline(string expectedDeclaration)
    {
        Assert.Contains(expectedDeclaration, VariablesCssContent, StringComparison.Ordinal);
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
