using BeeDayLab.Web.Components.Pages.ExperienceSystem;
using BeeDayLab.Web.Components.Pages.ExperienceSystem.Brand;
using BeeDayLab.Web.Components.Pages.ExperienceSystem.Components;
using BeeDayLab.Web.Components.Pages.ExperienceSystem.Ui;
using BeeDayLab.Web.Components.Pages.ExperienceSystem.Ux;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Deterministic bUnit component tests for Sprint 33.11 (FE33-069..076): the shared
/// ExperienceSystemPage/PillarNav/TopicNav/TopicGrid composition and the 20
/// /experience-system/* routes (1 root + 3 pillar overviews + 16 topic pages) that compose
/// through it. Every page KEEPS IStringLocalizer&lt;ExperienceSystemResources&gt; exactly as
/// production injects it (Sprint 33.11 policy reversal).
///
/// Correction vs. the Ledger note for FE33-076: the Ledger documents a NavigationManager
/// dependency for "active-nav highlighting", but as read from BeeDay.Web at extraction time no
/// component under Components/Features/ExperienceSystem/ actually injects NavigationManager —
/// active-nav highlighting is driven entirely by an explicit Current/CurrentHref parameter each
/// calling page hardcodes to match its own known @page route. The tests below exercise that real
/// mechanism (parameter-driven, not router-driven) rather than assuming the documented one.
/// </summary>
public sealed class ExperienceSystemPagesTests
{
    [Theory]
    [InlineData(ExperienceSystemPillar.Brand, "Brand System")]
    [InlineData(ExperienceSystemPillar.Ui, "UI Design System")]
    [InlineData(ExperienceSystemPillar.Ux, "UX System")]
    public void PillarNavMarksOnlyTheCurrentPillarLinkAsAriaCurrent(ExperienceSystemPillar current, string expectedLabel)
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        context.Services.AddLocalization();

        var cut = context.Render<ExperienceSystemPillarNav>(parameters => parameters
            .Add(p => p.Current, current));

        var currentLinks = cut.FindAll("a[aria-current='page']");
        Assert.Single(currentLinks);
        Assert.Equal(expectedLabel, currentLinks[0].TextContent);
    }

    [Fact]
    public void TopicNavMarksOnlyTheLinkMatchingCurrentHrefAsAriaCurrentDrivenByTheParameterNotTheRouter()
    {
        using var context = new BunitContext();
        context.Services.AddLocalization();

        var cut = context.Render<ExperienceSystemTopicNav>(parameters => parameters
            .Add(p => p.Pillar, ExperienceSystemPillar.Brand)
            .Add(p => p.CurrentHref, "/experience-system/brand/color"));

        var currentLink = cut.Find("a[aria-current='page']");
        Assert.Equal("/experience-system/brand/color", currentLink.GetAttribute("href"));
    }

    [Fact]
    public void ExperienceSystemPageRendersTheBrandGuidelinesHeroVariantInsteadOfThePageHeaderWhenSelected()
    {
        using var context = new BunitContext();
        context.Services.AddLocalization();

        var cut = context.Render<ExperienceSystemPage>(parameters => parameters
            .Add(p => p.Eyebrow, "Eyebrow")
            .Add(p => p.Title, "Title")
            .Add(p => p.ShowBrandGuidelinesHero, true));

        Assert.NotNull(cut.Find("header.beeday-hero"));
        Assert.Empty(cut.FindAll(".beeday-page-header"));
    }

    [Fact]
    public void ExperienceSystemPageRendersThePageHeaderAndSidebarTogglePillarNavWhenAPillarIsSet()
    {
        using var context = new BunitContext();
        context.Services.AddLocalization();

        var cut = context.Render<ExperienceSystemPage>(parameters => parameters
            .Add(p => p.Eyebrow, "Eyebrow")
            .Add(p => p.Title, "Title")
            .Add(p => p.Pillar, ExperienceSystemPillar.Ui));

        Assert.NotNull(cut.Find(".beeday-page-header"));
        Assert.Empty(cut.FindAll("header.beeday-hero"));
        Assert.NotNull(cut.Find("details.experience-system-sidebar summary.experience-system-sidebar__toggle"));
        Assert.NotNull(cut.Find("nav.experience-system-pillar-nav"));
    }

    /// <summary>All 20 /experience-system/* routes in this Sprint's scope render without throwing —
    /// one root, 3 pillar overviews, and 16 topic pages.</summary>
    [Fact]
    public void AllTwentyExperienceSystemRoutesRenderSuccessfully()
    {
        using var context = new BunitContext();
        context.Services.AddLocalization();

        context.Render<ExperienceSystemHome>();

        context.Render<BrandOverview>();
        context.Render<BrandIdentity>();
        context.Render<BrandWordmark>();
        context.Render<BrandColor>();
        context.Render<BrandIllustration>();
        context.Render<BrandCharacters>();
        context.Render<BrandWriting>();

        context.Render<UiOverview>();
        context.Render<UiFoundations>();
        context.Render<UiComponents>();
        context.Render<UiProductPatterns>();
        context.Render<UiInteraction>();
        context.Render<UiLayout>();

        context.Render<UxOverview>();
        context.Render<UxAccessibility>();
        context.Render<UxResponsive>();
        context.Render<UxLocalization>();
        context.Render<UxMotion>();
        context.Render<UxPerformance>();
    }

    [Fact]
    public void BrandIdentityRendersTheRealBrandTaxonomyHeadingAndMarksItsOwnTopicNavLinkCurrent()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        context.Services.AddLocalization();

        var cut = context.Render<BrandIdentity>();

        Assert.Equal("Identity", cut.Find(".beeday-page-header h1").TextContent);

        var currentLink = cut.Find("nav.experience-system-topic-nav a[aria-current='page']");
        Assert.Equal("/experience-system/brand/identity", currentLink.GetAttribute("href"));
    }
}
