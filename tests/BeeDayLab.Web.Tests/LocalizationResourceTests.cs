using System.Globalization;
using BeeDayLab.Web.Components.DesignSystem;
using BeeDayLab.Web.Components.Layout;
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
