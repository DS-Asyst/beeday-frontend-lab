using System.Reflection;
using BeeDayLab.Web.Components.Pages.Identity;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Sprint 33.12 (FE33-078, FE33-085, FE33-087) tests for Welcome.razor, Tutorial.razor's 5 static
/// slides + adapted final CTA, and RedirectToLogin's preview reachability.
/// </summary>
public sealed class OnboardingAndRedirectTests
{
    [Fact]
    public void WelcomeIsRoutedAtWelcomePath()
    {
        var routes = typeof(Welcome).GetCustomAttributes<RouteAttribute>(inherit: false);
        Assert.Contains(routes, r => r.Template == "/welcome");
    }

    [Fact]
    public void WelcomeRendersTheRedirectingMessage()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        context.Services.AddLocalization();

        var cut = context.Render<Welcome>();

        Assert.NotNull(cut.Find(".entry-loading"));
    }

    [Fact]
    public void TutorialIsRoutedAtOnboardingTutorialPath()
    {
        var routes = typeof(Tutorial).GetCustomAttributes<RouteAttribute>(inherit: false);
        Assert.Contains(routes, r => r.Template == "/onboarding/tutorial");
    }

    [Fact]
    public void TutorialStartsOnSlideOneOfFiveWithBackDisabled()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        context.Services.AddLocalization();

        var cut = context.Render<Tutorial>();

        Assert.Contains("1 OF 5", cut.Find(".tutorial-progress").TextContent);
        var backButton = cut.Find("button.beeday-button--back");
        Assert.True(backButton.HasAttribute("disabled"));
    }

    [Fact]
    public void TutorialAdvancesThroughAllFiveSlidesThenShowsEnterBeeDayOnTheLast()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        context.Services.AddLocalization();

        var cut = context.Render<Tutorial>();
        var nextButton = () => cut.FindAll("button.beeday-button--primary")[0];

        for (var i = 0; i < 4; i++)
        {
            nextButton().Click();
        }

        Assert.Contains("5 OF 5", cut.Find(".tutorial-progress").TextContent);
        Assert.Equal("ENTER beeday", nextButton().TextContent.Trim());
    }

    [Fact]
    public void TutorialFinalCtaNavigatesToProfileWithoutAnyRealStoreDependency()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        context.Services.AddLocalization();
        var navigation = context.Services.GetRequiredService<NavigationManager>();

        var cut = context.Render<Tutorial>();
        var nextButton = () => cut.FindAll("button.beeday-button--primary")[0];

        for (var i = 0; i < 4; i++)
        {
            nextButton().Click();
        }

        nextButton().Click();

        cut.WaitForAssertion(() => Assert.EndsWith("/profile", navigation.Uri), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void RedirectToLoginPreviewIsRoutedAtItsOwnExplicitDemoPath()
    {
        var routes = typeof(RedirectToLoginPreview).GetCustomAttributes<RouteAttribute>(inherit: false);
        Assert.Contains(routes, r => r.Template == "/identity/redirect-to-login-preview");
    }

    [Fact]
    public void RedirectToLoginPreviewRendersWithoutNavigatingAway()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        context.Services.AddLocalization();
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        var initialUri = navigation.Uri;

        var cut = context.Render<RedirectToLoginPreview>();

        // Per the Ledger note ("Lab representa a tela sem navegação forçada real"), rendering this
        // preview page must never actually navigate anywhere — unlike production's RedirectToLogin,
        // which force-navigates the instant it renders.
        Assert.Equal(initialUri, navigation.Uri);
        Assert.NotNull(cut.Find(".entry-loading"));
    }
}
