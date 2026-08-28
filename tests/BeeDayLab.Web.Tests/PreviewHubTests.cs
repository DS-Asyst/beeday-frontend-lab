using BeeDayLab.Web.Components.Pages.Preview;
using BeeDayLab.Web.Scenarios;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using PreviewPage = BeeDayLab.Web.Components.Pages.Preview.PreviewHub;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Sprint 33.17 (Issue #378): the shared toolbar must drive the SAME, already-existing
/// <see cref="ScenarioSelection"/> service the rest of the Lab already consumes (no second
/// scenario engine), and every registered page must be a real, reachable link — never a
/// screenshot placeholder.
/// </summary>
public sealed class PreviewHubTests
{
    [Fact]
    public void StateAndViewportSelectorsDriveTheSharedScenarioSelectionService()
    {
        using var context = CreateContext();
        var selection = context.Services.GetRequiredService<ScenarioSelection>();
        var cut = context.Render<PreviewPage>();

        Assert.Equal(ScenarioState.Populated, selection.State);
        Assert.Null(selection.Viewport);

        cut.Find("[data-testid='preview-state-select']").Change(nameof(ScenarioState.Error));
        Assert.Equal(ScenarioState.Error, selection.State);

        cut.Find("[data-testid='preview-viewport-select']").Change(nameof(ViewportPreset.Mobile));
        Assert.Equal(ViewportPreset.Mobile, selection.Viewport);
    }

    [Fact]
    public void EveryAreaSectionListsExactlyItsRegisteredPagesAsRealLinks()
    {
        using var context = CreateContext();
        var cut = context.Render<PreviewPage>();

        AssertSectionHasLinks(cut, "preview-public", PreviewPageRegistry.Public);
        AssertSectionHasLinks(cut, "preview-identity", PreviewPageRegistry.Identity);
        AssertSectionHasLinks(cut, "preview-account", PreviewPageRegistry.Account);
        AssertSectionHasLinks(cut, "preview-daily", PreviewPageRegistry.Daily);
        AssertSectionHasLinks(cut, "preview-wallet", PreviewPageRegistry.Wallet);
        AssertSectionHasLinks(cut, "preview-email", PreviewPageRegistry.Email);
        AssertSectionHasLinks(cut, "preview-system", PreviewPageRegistry.System);
    }

    [Fact]
    public void ResponsivePreviewRendersThreeRealFramesAtTheDesignSystemBreakpointsForTheChosenPage()
    {
        using var context = CreateContext();
        var cut = context.Render<PreviewPage>();

        cut.Find("[data-testid='preview-responsive-select']").Change("/emails?template=reset");

        var mobile = cut.Find("[data-testid='preview-frame-mobile']");
        var tablet = cut.Find("[data-testid='preview-frame-tablet']");
        var desktop = cut.Find("[data-testid='preview-frame-desktop']");

        Assert.Equal("/emails?template=reset", mobile.GetAttribute("src"));
        Assert.Equal("/emails?template=reset", tablet.GetAttribute("src"));
        Assert.Equal("/emails?template=reset", desktop.GetAttribute("src"));

        Assert.Contains("max-width:375px", mobile.ParentElement!.GetAttribute("style"), StringComparison.Ordinal);
        Assert.Contains("max-width:768px", tablet.ParentElement!.GetAttribute("style"), StringComparison.Ordinal);
        Assert.Contains("max-width:1280px", desktop.ParentElement!.GetAttribute("style"), StringComparison.Ordinal);
    }

    [Fact]
    public void PublicLanguageSwitcherIsTheRealSharedComponentNotAReinventedOne()
    {
        using var context = CreateContext();
        var cut = context.Render<PreviewPage>();

        Assert.NotEmpty(cut.FindAll(".public-language-switcher"));
        Assert.Equal(2, cut.FindAll(".public-language-switcher__option").Count);
    }

    private static void AssertSectionHasLinks<TComponent>(IRenderedComponent<TComponent> cut, string sectionId, PreviewPageEntry[] expected)
        where TComponent : Microsoft.AspNetCore.Components.IComponent
    {
        var links = cut.Find($"[data-testid='preview-index-{sectionId}']").QuerySelectorAll("a");
        Assert.Equal(expected.Length, links.Length);

        foreach (var entry in expected)
        {
            Assert.Contains(links, a => a.GetAttribute("href") == entry.Path);
        }
    }

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddLocalization();
        context.Services.AddScoped<ScenarioSelection>();
        return context;
    }
}
