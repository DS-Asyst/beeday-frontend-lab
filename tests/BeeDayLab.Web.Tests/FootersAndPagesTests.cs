using BeeDayLab.Web.Components.Layout;
using BeeDayLab.Web.Components.Pages;
using Bunit;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Deterministic bUnit component tests for Sprint 33.9 (FE33-047..052): proves the public header,
/// both footers, the MOCKed ReconnectModal, and the NotFound/Error pages render their hardcoded
/// English strings and Lab-local scenario/state parameters correctly, with none of the production
/// dependencies (IStringLocalizer&lt;SharedResources&gt;/&lt;LayoutResources&gt;,
/// AuthenticatedEntryDestinationResolver, real AuthorizeView/AuthenticationStateProvider,
/// HttpContext, real SignalR reconnection JS) wired to anything.
/// </summary>
public sealed class FootersAndPagesTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PublicHeaderShowsTheContinueCtaOnlyWhenIsAuthenticatedScenarioIsTrue(bool isAuthenticatedScenario)
    {
        using var context = new BunitContext();

        var cut = context.Render<PublicHeader>(parameters => parameters
            .Add(p => p.IsAuthenticatedScenario, isAuthenticatedScenario));

        Assert.Equal("beeday home", cut.Find("a.public-header__brand").GetAttribute("aria-label"));
        Assert.Equal(isAuthenticatedScenario, cut.FindAll("button.beeday-button").Count > 0);

        if (isAuthenticatedScenario)
        {
            Assert.Contains("Continue to beeday", cut.Markup);
        }
        else
        {
            Assert.DoesNotContain("Continue to beeday", cut.Markup);
        }
    }

    [Fact]
    public void PublicLanguageSwitcherRendersGroupAriaLabelAndBothCultureOptions()
    {
        using var context = new BunitContext();

        var cut = context.Render<PublicLanguageSwitcher>();

        var form = cut.Find("form.public-language-switcher");
        Assert.Equal("Language", form.GetAttribute("aria-label"));
        Assert.Equal("/culture/set", form.GetAttribute("action"));

        var options = cut.FindAll("button.public-language-switcher__option");
        Assert.Equal(2, options.Count);
        Assert.Equal("pt-BR", options[0].GetAttribute("value"));
        Assert.Equal("en-US", options[1].GetAttribute("value"));
    }

    [Fact]
    public void AppFooterRendersHardcodedCopyAndTheNonLinkSocialPlaceholders()
    {
        using var context = new BunitContext();

        var cut = context.Render<AppFooter>();

        Assert.Contains("Be Better Every Day", cut.Markup);
        Assert.Contains("© 2026 beeday. All rights reserved.", cut.Markup);

        // No real Instagram/X URL exists yet — both render as non-interactive placeholders, not links.
        var instagramPlaceholder = cut.Find("span[aria-label='Instagram (coming soon)']");
        Assert.Equal("app-footer__social-unavailable", instagramPlaceholder.ClassName);
        var xPlaceholder = cut.Find("span[aria-label='X (coming soon)']");
        Assert.Equal("app-footer__social-unavailable", xPlaceholder.ClassName);

        // LinkedIn is the one real social link.
        var linkedIn = cut.Find("a[aria-label='LinkedIn']");
        Assert.Equal("https://www.linkedin.com/in/tiago-a-arrigoni-335b9413b/", linkedIn.GetAttribute("href"));
    }

    [Fact]
    public void EditorialFooterBackToTopInvokesTheQueryStringFreeScrollModule()
    {
        using var context = new BunitContext();
        var module = context.JSInterop.SetupModule("./js/beeday-editorial-footer.js");
        module.SetupVoid("scrollToTop", _ => true);

        var cut = context.Render<EditorialFooter>();

        Assert.Contains("BUY ME A COFFEE", cut.Markup);
        var backToTop = cut.Find("button.editorial-footer__back-to-top");
        Assert.Equal("Back to top", backToTop.GetAttribute("aria-label"));

        // No JSException/InvalidOperationException means the exact configured, query-string-free
        // module path ("./js/beeday-editorial-footer.js") was the one actually imported.
        backToTop.Click();
    }

    [Theory]
    [InlineData(ReconnectDisplayState.Hidden, false, null)]
    [InlineData(ReconnectDisplayState.Rejoining, true, "components-reconnect-show")]
    [InlineData(ReconnectDisplayState.Retrying, true, "components-reconnect-retrying")]
    [InlineData(ReconnectDisplayState.Failed, true, "components-reconnect-failed")]
    [InlineData(ReconnectDisplayState.Paused, true, "components-reconnect-paused")]
    [InlineData(ReconnectDisplayState.ResumeFailed, true, "components-reconnect-resume-failed")]
    public void ReconnectModalRendersTheDialogOpenAttributeAndStateClassForEachState(
        ReconnectDisplayState state, bool expectedOpen, string? expectedClass)
    {
        using var context = new BunitContext();

        var cut = context.Render<ReconnectModal>(parameters => parameters
            .Add(p => p.State, state));

        var dialog = cut.Find("dialog#components-reconnect-modal");
        Assert.Equal(expectedOpen, dialog.HasAttribute("open"));

        if (expectedClass is not null)
        {
            Assert.Contains(expectedClass, dialog.ClassList);
        }
        else
        {
            Assert.Empty(dialog.ClassList);
        }
    }

    [Fact]
    public void ReconnectModalDefaultsToHiddenWithNoParameterSupplied()
    {
        using var context = new BunitContext();

        var cut = context.Render<ReconnectModal>();

        Assert.False(cut.Find("dialog#components-reconnect-modal").HasAttribute("open"));
    }

    [Fact]
    public void NotFoundPageRendersHardcodedEnglishStringsWithNoLocalizerDependency()
    {
        using var context = new BunitContext();

        var cut = context.Render<NotFound>();

        Assert.Equal("Not Found", cut.Find("h1").TextContent);
        Assert.Contains("Sorry, the content you are looking for does not exist.", cut.Markup);
    }

    [Fact]
    public void ErrorPageRendersASyntheticTraceIdWithNoHttpContextOrActivityDependency()
    {
        using var context = new BunitContext();

        var cut = context.Render<Error>();

        Assert.Equal("Error", cut.Find("h1").TextContent);
        Assert.Contains("An error occurred while processing your request.", cut.Markup);

        // Fixed, obviously-synthetic placeholder — not derived from any real HttpContext/Activity.
        var requestIdCode = cut.Find("code");
        Assert.Equal("00000000-0000-0000-0000-000000000000", requestIdCode.TextContent);
    }
}
