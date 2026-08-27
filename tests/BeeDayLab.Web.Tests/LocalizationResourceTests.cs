using System.Globalization;
using BeeDayLab.Web.Components.DesignSystem;
using BeeDayLab.Web.Components.Layout;
using BeeDayLab.Web.Components.Pages.ExperienceSystem;
using BeeDayLab.Web.Components.Pages.Institutional;
using BeeDayLab.Web.Components.Pages.Public;
using BeeDayLab.Web.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Integration-style checks (Sprint 33.10, FE33-104) proving the three resx catalogs copied
/// verbatim from BeeDay.Web (SharedResources, LayoutResources, DesignSystemResources) actually
/// resolve real strings through <see cref="IStringLocalizer{T}"/> for both supported cultures,
/// using the standard <c>AddLocalization()</c> + marker-type-by-namespace convention — no
/// designer-generated .cs needed, matching how BeeDay.Web itself resolves these same catalogs.
/// </summary>
public sealed class LocalizationResourceTests
{
    [Theory]
    [InlineData("en-US", "Language")]
    [InlineData("pt-BR", "Idioma")]
    public void SharedResourcesResolvesLanguageSwitcherLabelForCulture(string culture, string expected)
    {
        using var scope = new CultureScope(culture);
        var localizer = BuildLocalizer<SharedResources>();

        Assert.Equal(expected, localizer["LanguageSwitcherGroupAriaLabel"].Value);
    }

    [Theory]
    [InlineData("en-US", "Skip to content")]
    [InlineData("pt-BR", "Pular para o conteúdo")]
    public void LayoutResourcesResolvesSkipToContentLabelForCulture(string culture, string expected)
    {
        using var scope = new CultureScope(culture);
        var localizer = BuildLocalizer<LayoutResources>();

        Assert.Equal(expected, localizer["SkipToContentLabel"].Value);
    }

    [Theory]
    [InlineData("en-US", "Confirm")]
    [InlineData("pt-BR", "Confirmar")]
    public void DesignSystemResourcesResolvesConfirmDialogLabelForCulture(string culture, string expected)
    {
        using var scope = new CultureScope(culture);
        var localizer = BuildLocalizer<DesignSystemResources>();

        Assert.Equal(expected, localizer["ConfirmDialogConfirmLabel"].Value);
    }

    // Sprint 33.11 (FE33-053..076): four new resx catalog families, copied verbatim mirroring this
    // same Sprint 33.10 convention — kept, not stripped, per the Sprint 33.11 policy reversal (the
    // Lab now has a real localization pipeline). One representative key per catalog, both cultures.

    [Theory]
    [InlineData("en-US", "beeday — Be better every day")]
    [InlineData("pt-BR", "beeday — Seja melhor a cada dia")]
    public void HomeResourcesResolvesPageTitleForCulture(string culture, string expected)
    {
        using var scope = new CultureScope(culture);
        var localizer = BuildLocalizer<HomeResources>();

        Assert.Equal(expected, localizer["PageTitle"].Value);
    }

    [Theory]
    [InlineData("en-US", "Typography with purpose")]
    [InlineData("pt-BR", "Tipografia com propósito")]
    public void BrandTypographyResourcesResolvesTitleForCulture(string culture, string expected)
    {
        using var scope = new CultureScope(culture);
        var localizer = BuildLocalizer<BrandTypographyResources>();

        Assert.Equal(expected, localizer["Title"].Value);
    }

    [Theory]
    [InlineData("en-US", "Our mission")]
    [InlineData("pt-BR", "Nossa missão")]
    public void InstitutionalResourcesResolvesMissionTitleForCulture(string culture, string expected)
    {
        using var scope = new CultureScope(culture);
        var localizer = BuildLocalizer<InstitutionalResources>();

        Assert.Equal(expected, localizer["MissionTitle"].Value);
    }

    [Theory]
    [InlineData("en-US", "Brand System")]
    [InlineData("pt-BR", "Brand System")]
    public void ExperienceSystemResourcesResolvesPillarNavBrandLabelForCulture(string culture, string expected)
    {
        using var scope = new CultureScope(culture);
        var localizer = BuildLocalizer<ExperienceSystemResources>();

        Assert.Equal(expected, localizer["PillarNavBrand"].Value);
    }

    private static IStringLocalizer<T> BuildLocalizer<T>()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        return services.BuildServiceProvider().GetRequiredService<IStringLocalizer<T>>();
    }

    /// <summary>Sets <see cref="CultureInfo.CurrentUICulture"/> for the duration of one test and restores it afterward.</summary>
    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo originalCulture;
        private readonly CultureInfo originalUiCulture;

        public CultureScope(string culture)
        {
            originalCulture = CultureInfo.CurrentCulture;
            originalUiCulture = CultureInfo.CurrentUICulture;

            var target = new CultureInfo(culture);
            CultureInfo.CurrentCulture = target;
            CultureInfo.CurrentUICulture = target;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }
}
