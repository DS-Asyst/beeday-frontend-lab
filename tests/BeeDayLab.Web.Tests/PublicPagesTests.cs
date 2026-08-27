using System.Reflection;
using BeeDayLab.Web.Components.Pages.Public;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Deterministic bUnit component tests for Sprint 33.11 (FE33-053, FE33-068): the two pages that
/// land directly under Components/Pages/Public/. Unlike Sprints 33.8/33.9, this Sprint KEEPS
/// IStringLocalizer&lt;T&gt; injections exactly as production has them (Issue #372 work item 3) —
/// every test below registers a real AddLocalization() pipeline, forces en-US via
/// <see cref="TestCultureScope"/> (so results are deterministic regardless of the host runner's
/// own default culture), and asserts against real en-US.resx strings, never hardcoded copy.
/// </summary>
public sealed class PublicPagesTests
{
    [Fact]
    public void HomeIsRoutedAtTheRootPath()
    {
        var routes = typeof(Home).GetCustomAttributes<RouteAttribute>(inherit: false);

        Assert.Contains(routes, r => r.Template == "/");
    }

    [Fact]
    public void HomeRendersTheAnonymousCtasByDefault()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        context.Services.AddLocalization();

        var cut = context.Render<Home>();

        Assert.Equal("Build a better day, one step at a time.", cut.Find("h1").TextContent);
        Assert.NotNull(cut.Find("a.home-hero__start"));
        Assert.NotNull(cut.Find("a.home-hero__login"));
        Assert.Equal("Get started", cut.Find("a.home-hero__start").TextContent);
        Assert.Equal("I already have an account", cut.Find("a.home-hero__login").TextContent);
        Assert.Empty(cut.FindAll("button.beeday-button"));
    }

    [Fact]
    public void HomeRendersTheAuthenticatedContinueCtaWhenTheAuthenticatedScenarioIsSelected()
    {
        // Lab-local scenario toggle (Sprint 33.11) standing in for the real
        // <AuthorizeView>/AuthenticatedEntryDestinationResolver — previewed via
        // "/?authenticated=true" in a real browser. bUnit requires SupplyParameterFromQuery
        // parameters to be supplied by navigating the fake NavigationManager to that query string
        // before rendering, rather than set directly like a plain [Parameter].
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        context.Services.AddLocalization();
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("authenticated", true));

        var cut = context.Render<Home>();

        Assert.Empty(cut.FindAll("a.home-hero__start"));
        Assert.Empty(cut.FindAll("a.home-hero__login"));

        var button = cut.Find("button.beeday-button");
        Assert.Equal("Continue to beeday", button.TextContent);
    }

    [Fact]
    public void TypographyGuidelinesIsRoutedAtBothProductionPaths()
    {
        var routes = typeof(TypographyGuidelines).GetCustomAttributes<RouteAttribute>(inherit: false)
            .Select(r => r.Template)
            .ToList();

        Assert.Contains("/brand/typography", routes);
        Assert.Contains("/experience-system/brand/typography", routes);
    }

    [Fact]
    public void TypographyGuidelinesRendersThePillarAndTopicNavPlusRealLocalizedHeading()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        context.Services.AddLocalization();

        var cut = context.Render<TypographyGuidelines>();

        Assert.NotNull(cut.Find("nav.experience-system-pillar-nav"));
        Assert.NotNull(cut.Find("nav.experience-system-topic-nav"));
        Assert.Equal("Typography with purpose", cut.Find("article.brand-typography h1").TextContent);

        var currentTopicLink = cut.Find("nav.experience-system-topic-nav a[aria-current='page']");
        Assert.EndsWith("/experience-system/brand/typography", currentTopicLink.GetAttribute("href"));
    }
}
