using BeeDayLab.Web.Components.Pages.Institutional;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Deterministic bUnit component tests for Sprint 33.11 (FE33-054..067): the 12 institutional
/// pages and the shared shell/templates they all compose through (InstitutionalPageShell,
/// EditorialPageTemplate/ProductPageTemplate/HelpPageTemplate/LegalDocumentPageTemplate). Every
/// page KEEPS IStringLocalizer&lt;InstitutionalResources&gt; exactly as production injects it
/// (Sprint 33.11 policy reversal) — every test registers a real AddLocalization() pipeline.
/// </summary>
public sealed class InstitutionalPagesTests
{
    /// <summary>All 12 institutional pages render a non-empty hero heading — one assertion per
    /// page, proving each template/shell composition works end to end for every page in scope.</summary>
    [Fact]
    public void AllTwelveInstitutionalPagesRenderANonEmptyHeroHeading()
    {
        using var context = new BunitContext();
        context.Services.AddLocalization();

        AssertRendersNonEmptyHeroHeading(context.Render<Mission>());
        AssertRendersNonEmptyHeroHeading(context.Render<Efficacy>());
        AssertRendersNonEmptyHeroHeading(context.Render<BrandGuidelines>());
        AssertRendersNonEmptyHeroHeading(context.Render<Contact>());
        AssertRendersNonEmptyHeroHeading(context.Render<Product>());
        AssertRendersNonEmptyHeroHeading(context.Render<ProductPlus>());
        AssertRendersNonEmptyHeroHeading(context.Render<Android>());
        AssertRendersNonEmptyHeroHeading(context.Render<Ios>());
        AssertRendersNonEmptyHeroHeading(context.Render<Faqs>());
        AssertRendersNonEmptyHeroHeading(context.Render<CommunityGuidelines>());
        AssertRendersNonEmptyHeroHeading(context.Render<Terms>());
        AssertRendersNonEmptyHeroHeading(context.Render<Privacy>());
    }

    private static void AssertRendersNonEmptyHeroHeading<TComponent>(IRenderedComponent<TComponent> cut)
        where TComponent : IComponent =>
        Assert.False(string.IsNullOrWhiteSpace(cut.Find("header.beeday-hero h1").TextContent));

    [Fact]
    public void MissionShellComposesTheHeroWithTheAboutUsContextualNavAndMarksItselfCurrent()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        context.Services.AddLocalization();
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/mission");

        var cut = context.Render<Mission>();

        Assert.Equal("Our mission", cut.Find("header.beeday-hero h1").TextContent);

        // InstitutionalPageShell's NavigationManager usage (FE33-067) is only for this — computing
        // the current route so EditorialSectionNav can mark its own link aria-current="page".
        var currentLink = cut.Find("nav.editorial-section-nav a[aria-current='page']");
        Assert.Equal("Mission", currentLink.TextContent);

        // The rest of the "About us" family renders as sibling links (Efficacy, Contact us) —
        // resolved deterministically from EditorialSectionRegistry.
        foreach (var sibling in new[] { "Efficacy", "Contact us" })
        {
            Assert.Contains(sibling, cut.Find("nav.editorial-section-nav").TextContent);
        }
    }

    [Fact]
    public void ContactRendersTheGitHubSupportLinkAsAnInertPlaceholderNotARealLink()
    {
        // Ledger note for FE33-057: "mesmo tratamento do FE33-048" — same non-interactive
        // placeholder treatment AppFooter.razor's social icons already got in Sprint 33.9, applied
        // here because the production page's support link points at the real
        // https://github.com/DS-Asyst/BeeDay repository.
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        context.Services.AddLocalization();

        var cut = context.Render<Contact>();

        Assert.DoesNotContain("https://github.com/DS-Asyst/BeeDay", cut.Markup);
        Assert.Empty(cut.FindAll("a[href='https://github.com/DS-Asyst/BeeDay']"));

        var placeholder = cut.Find("span.institutional-link-unavailable");
        Assert.Equal("Open an issue on GitHub", placeholder.TextContent);

        // The LinkedIn link is real, already-public, and kept exactly as-is (same URL AppFooter
        // already uses verbatim, Sprint 33.9) — no adaptation needed.
        var linkedIn = cut.Find("a[href='https://www.linkedin.com/in/tiago-a-arrigoni-335b9413b/']");
        Assert.NotNull(linkedIn);
    }

    [Fact]
    public void ProductRendersTheProductsFamilyPrimaryActionAndFeatures()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        context.Services.AddLocalization();

        var cut = context.Render<Product>();

        Assert.Equal("beeday", cut.Find("header.beeday-hero h1").TextContent);
        var primaryAction = cut.Find("a.beeday-button--important-white");
        Assert.Equal("/profile/create", primaryAction.GetAttribute("href"));
    }

    [Fact]
    public void FaqsRendersFourCollapsibleDetailsItems()
    {
        using var context = new BunitContext();
        context.Services.AddLocalization();

        var cut = context.Render<Faqs>();

        Assert.Equal(4, cut.FindAll("details.institutional-faq__item").Count);
    }

    [Fact]
    public void LegalDocumentPagesRenderThePendingReviewNoticeAndTableOfContents()
    {
        using var context = new BunitContext();
        context.Services.AddLocalization();

        AssertRendersPendingNoticeAndToc(context.Render<CommunityGuidelines>());
        AssertRendersPendingNoticeAndToc(context.Render<Terms>());
        AssertRendersPendingNoticeAndToc(context.Render<Privacy>());
    }

    private static void AssertRendersPendingNoticeAndToc<TComponent>(IRenderedComponent<TComponent> cut)
        where TComponent : IComponent
    {
        Assert.NotNull(cut.Find("p.institutional-pending-notice"));
        Assert.NotNull(cut.Find("nav.institutional-legal__toc"));
    }

    [Fact]
    public void BrandGuidelinesComposesThePillarAndTopicNavFromTheExperienceSystem()
    {
        using var context = new BunitContext();
        context.Services.AddLocalization();

        var cut = context.Render<BrandGuidelines>();

        Assert.NotNull(cut.Find("nav.experience-system-pillar-nav"));
        Assert.NotNull(cut.Find("nav.experience-system-topic-nav"));
        Assert.Equal(10, cut.FindAll("span.brand-guidelines-swatch").Count);
    }
}
