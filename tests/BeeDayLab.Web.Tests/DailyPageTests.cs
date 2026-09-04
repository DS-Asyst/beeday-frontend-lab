using System.Reflection;
using BeeDayLab.Web.Components.DesignSystem.Feedback;
using BeeDayLab.Web.Components.Pages.Daily;
using BeeDayLab.Web.Components.Pages.Daily.Experience.Feedback;
using BeeDayLab.Web.Components.Pages.Daily.State;
using BeeDayLab.Web.Scenarios;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Sprint 33.13 (FE33-088/090) tests for the two Daily pages, <c>DailyHome</c> (/daily) and
/// <c>ProfileHome</c> (/profile). Both render independently across Empty/Populated/LargeContent, and
/// neither carries any authorization or real-auth dependency — production gates both behind
/// <c>[Authorize]</c> plus an <c>AuthenticatedUserInitializer</c>, all of which this Sprint dropped.
/// </summary>
public sealed class DailyPageTests
{
    [Fact]
    public void DailyAndProfileKeepTheirProductionRoutes()
    {
        Assert.Contains(
            "/daily",
            typeof(DailyHome).GetCustomAttributes<RouteAttribute>(inherit: false).Select(route => route.Template));

        Assert.Contains(
            "/profile",
            typeof(ProfileHome).GetCustomAttributes<RouteAttribute>(inherit: false).Select(route => route.Template));
    }

    [Fact]
    public void NeitherPageCarriesAnAuthorizeAttribute()
    {
        // Production declares @attribute [Authorize] on both pages. Asserted by attribute type NAME
        // rather than by referencing AuthorizeAttribute: the Lab does not reference
        // Microsoft.AspNetCore.Authorization at all, so naming the type here would not even compile —
        // which is the stronger statement, and this check keeps holding if that ever changes.
        foreach (var pageType in new[] { typeof(DailyHome), typeof(ProfileHome) })
        {
            var attributeNames = pageType.GetCustomAttributes(inherit: true)
                .Select(attribute => attribute.GetType().Name)
                .ToList();

            Assert.DoesNotContain("AuthorizeAttribute", attributeNames);
        }
    }

    [Fact]
    public void NeitherPageInjectsAnyAuthenticationOrAuthorizationService()
    {
        // Production injects AuthenticatedUserInitializer (and DashboardHome additionally reaches an
        // AuthenticationStateProvider through it) to decide whether the visitor has a profile yet.
        // Both were dropped: the Lab has no account concept. Asserted over each page's actual
        // [Inject] members, which is what "no real auth dependency" concretely means here.
        foreach (var pageType in new[] { typeof(DailyHome), typeof(ProfileHome) })
        {
            var injectedTypes = pageType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(property => property.GetCustomAttributes()
                    .Any(attribute => attribute.GetType().Name == "InjectAttribute"))
                .Select(property => property.PropertyType.FullName ?? property.PropertyType.Name)
                .ToList();

            Assert.NotEmpty(injectedTypes);
            Assert.DoesNotContain(injectedTypes, type => type.Contains("Authentication", StringComparison.Ordinal));
            Assert.DoesNotContain(injectedTypes, type => type.Contains("Authorization", StringComparison.Ordinal));
            Assert.DoesNotContain(injectedTypes, type => type.Contains("AuthenticatedUserInitializer", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void BothPagesRenderWithoutAnyAuthenticationBeingConfigured()
    {
        // CreateContext registers only localization, ToastService, the scenario engine and the two
        // Daily-scoped services — no authorization at all. A page still reaching for real auth would
        // fail here at render time.
        using var culture = new TestCultureScope();
        using var context = CreateContext(ScenarioState.Populated);

        var cut = context.Render<DailyHome>();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".dashboard-grid")), TimeSpan.FromSeconds(3));
    }

    [Theory]
    [InlineData(ScenarioState.Populated)]
    [InlineData(ScenarioState.Empty)]
    [InlineData(ScenarioState.LargeContent)]
    public void DailyRendersItsFourColumnsForEveryContentScenario(ScenarioState scenarioState)
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext(scenarioState);

        var cut = context.Render<DailyHome>();

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".dashboard-grid")), TimeSpan.FromSeconds(3));
        Assert.Equal(4, cut.FindAll(".dashboard-column").Count);

        // The filter bar is always present — it is how the "no filter results" states are reachable.
        Assert.NotEmpty(cut.FindAll("#dashboard-search"));
    }

    [Fact]
    public void DailyHasNoVisibleHeroOrPageHeaderAndKeepsAVisuallyHiddenHeadingAboveItsUnchangedBoard()
    {
        // Sprint 35.1-R3 (OWNER direction change): the OWNER rejected the authenticated Hero
        // direction (EPIC 35 Sprint 35.1-R2's WorkspaceHero) for a top-navigation-led shell — Daily is
        // an operational/productivity page and does not get a page-level Hero or BeeDayPageHeader of
        // any kind; the top navigation already shows Daily as the active destination. This restores
        // the pre-Sprint-35.1 contract: a visually-hidden <h1>DailyHeading</h1> (a real heading stays
        // in the accessibility tree without adding visible page furniture above the board), with the
        // filter bar and boards exactly where they were.
        using var culture = new TestCultureScope();
        using var context = CreateContext(ScenarioState.Populated);

        var cut = context.Render<DailyHome>();

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".dashboard-grid")), TimeSpan.FromSeconds(3));

        Assert.Empty(cut.FindAll("header.beeday-hero"));
        Assert.Empty(cut.FindAll(".beeday-page-header"));

        var heading = cut.Find("h1");
        Assert.Equal("Daily", heading.TextContent);
        Assert.Contains("beeday-visually-hidden", heading.ClassList);
        Assert.Single(cut.FindAll("h1"));

        var headingPosition = cut.Markup.IndexOf("beeday-visually-hidden", StringComparison.Ordinal);
        var boardPosition = cut.Markup.IndexOf("dashboard-grid", StringComparison.Ordinal);
        Assert.True(headingPosition >= 0 && boardPosition > headingPosition);
    }

    [Fact]
    public void DailyShowsEmptyStatesRatherThanCardsForTheEmptyScenario()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext(ScenarioState.Empty);

        var cut = context.Render<DailyHome>();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".dashboard-grid")), TimeSpan.FromSeconds(3));

        Assert.Empty(cut.FindAll(".activity-card"));
        Assert.Empty(cut.FindAll(".habit-card"));
        Assert.NotEmpty(cut.FindAll(".dashboard-column__empty"));
    }

    [Fact]
    public void DailyRendersHabitAndActivityCardsForThePopulatedScenario()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext(ScenarioState.Populated);

        var cut = context.Render<DailyHome>();
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".habit-card")), TimeSpan.FromSeconds(3));

        // Seven habits, one per HabitVisualState band — the banded class must reach the DOM.
        Assert.Equal(7, cut.FindAll(".habit-card").Count);
        Assert.Contains("habit-card--sky", cut.Markup);
        Assert.Contains("habit-card--red-strong", cut.Markup);
        Assert.Contains("habit-card--white", cut.Markup);
        Assert.NotEmpty(cut.FindAll(".activity-card"));
    }

    [Fact]
    public void DailyRendersTheLoadingSkeletonForTheLoadingScenario()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext(ScenarioState.Loading);

        var cut = context.Render<DailyHome>();

        Assert.NotEmpty(cut.FindAll(".dashboard-skeleton"));
        Assert.Empty(cut.FindAll(".dashboard-grid"));
    }

    [Fact]
    public void DailyRendersTheUnavailablePanelForTheErrorScenario()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext(ScenarioState.Error);

        var cut = context.Render<DailyHome>();

        cut.WaitForAssertion(
            () => Assert.NotEmpty(cut.FindAll("#daily-unavailable-heading")),
            TimeSpan.FromSeconds(3));
        Assert.Empty(cut.FindAll(".dashboard-grid"));

        var toast = context.Services.GetRequiredService<ToastService>();
        Assert.NotEmpty(toast.Messages);
        Assert.Equal(ToastVariant.Error, toast.Messages[^1].Variant);
    }

    [Theory]
    [InlineData(ScenarioState.Populated)]
    [InlineData(ScenarioState.Empty)]
    [InlineData(ScenarioState.LargeContent)]
    public void ProfileRendersItsWelcomeSummaryForEveryContentScenario(ScenarioState scenarioState)
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext(scenarioState);

        var cut = context.Render<ProfileHome>();

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".product-home")), TimeSpan.FromSeconds(3));

        // The experience card is the page's headline element and reads pre-resolved scenario values.
        Assert.NotEmpty(cut.FindAll(".experience-card"));
    }

    [Fact]
    public void ProfileShowsTheActiveProjectCardOnlyWhenAnActiveProjectExists()
    {
        using var culture = new TestCultureScope();

        using (var populated = CreateContext(ScenarioState.Populated))
        {
            var cut = populated.Render<ProfileHome>();
            cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".product-home")), TimeSpan.FromSeconds(3));
            Assert.NotEmpty(cut.FindAll(".product-home__project"));
        }

        using (var empty = CreateContext(ScenarioState.Empty))
        {
            var cut = empty.Render<ProfileHome>();
            cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".product-home")), TimeSpan.FromSeconds(3));
            Assert.Empty(cut.FindAll(".product-home__project"));
        }
    }

    [Fact]
    public void ProfileRendersItsWelcomeSummaryAsAContentFirstPageHeaderInsideProductHome()
    {
        // Sprint 35.1-R3 (OWNER direction change): the OWNER rejected the authenticated full-width
        // Hero direction (EPIC 35 Sprint 35.1-R2's WorkspaceHero) for a top-navigation-led shell, so
        // the welcome summary returns to a content-first BeeDayPageHeader nested inside .product-home
        // — its original, pre-Sprint-35.1 composition — sharing that section's max-width:64rem with
        // the cards below it instead of spanning the full authenticated workspace width.
        using var culture = new TestCultureScope();
        using var context = CreateContext(ScenarioState.Populated);

        var cut = context.Render<ProfileHome>();

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll(".beeday-page-header")), TimeSpan.FromSeconds(3));

        Assert.Empty(cut.FindAll("header.beeday-hero"));

        var productHome = cut.Find("section.product-home");
        var header = productHome.QuerySelector(".beeday-page-header");
        Assert.NotNull(header);
        Assert.Equal("Welcome back, jordan.silva", header!.QuerySelector("h1")!.TextContent);
        Assert.Contains("Choose one next step and keep your day moving.", header.TextContent);
        Assert.NotNull(header.QuerySelector(".beeday-page-header__actions .beeday-button"));

        // The page header is the first thing inside .product-home, ahead of the progress card.
        var headerPosition = cut.Markup.IndexOf("beeday-page-header", StringComparison.Ordinal);
        var progressPosition = cut.Markup.IndexOf("product-home__progress", StringComparison.Ordinal);
        Assert.True(headerPosition >= 0 && progressPosition > headerPosition);
    }

    [Fact]
    public void ProfileRendersTheUnavailablePanelForTheErrorScenario()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext(ScenarioState.Error);

        var cut = context.Render<ProfileHome>();

        cut.WaitForAssertion(
            () => Assert.NotEmpty(cut.FindAll("#home-unavailable-heading")),
            TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void TheTwoPagesRenderIndependentlyOfEachOther()
    {
        // Each page owns its own circuit-scoped LabDashboardState; neither needs the other to have
        // run first (production's /profile could redirect into /profile/create, which is gone).
        using var culture = new TestCultureScope();

        using var profileOnly = CreateContext(ScenarioState.Populated);
        var profile = profileOnly.Render<ProfileHome>();
        profile.WaitForAssertion(() => Assert.NotEmpty(profile.FindAll(".product-home")), TimeSpan.FromSeconds(3));

        using var dailyOnly = CreateContext(ScenarioState.Populated);
        var daily = dailyOnly.Render<DailyHome>();
        daily.WaitForAssertion(() => Assert.NotEmpty(daily.FindAll(".dashboard-grid")), TimeSpan.FromSeconds(3));
    }

    internal static BunitContext CreateContext(ScenarioState scenarioState)
    {
        var context = new BunitContext();

        var sortable = context.JSInterop.SetupModule("./js/beeday-sortable.js");
        sortable.SetupVoid("initialize", _ => true);
        sortable.SetupVoid("dispose", _ => true);

        var dialogFocus = context.JSInterop.SetupModule("./js/beeday-dialog-focus.js");
        dialogFocus.SetupVoid("deactivate", _ => true);
        dialogFocus.Setup<bool>("activate", _ => true).SetResult(true);
        dialogFocus.SetupVoid("focusFirstInvalid", _ => true);

        context.Services.AddLocalization();
        context.Services.AddScoped<ToastService>();
        context.Services.AddScoped(_ => new ScenarioSelection { State = scenarioState });
        context.Services.AddSingleton<DailyDashboardScenarioProvider>();
        context.Services.AddScoped<BeeDayFeedbackStore>();
        context.Services.AddScoped<LabDashboardState>();

        return context;
    }
}
