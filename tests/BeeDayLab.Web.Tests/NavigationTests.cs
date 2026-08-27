using BeeDayLab.Web.Components.DesignSystem.Icons;
using BeeDayLab.Web.Components.Layout;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Deterministic bUnit component tests for Sprint 33.9 (FE33-043..046): proves the authenticated
/// navigation chrome (DesktopSidebar, MobileHeader, MobileSidebar, NavigationItem, NavigationItems)
/// renders its accessibility wiring (aria-label, aria-expanded, aria-hidden, aria-current) and real
/// focus-management behavior (MobileHeader's focus-return-on-close, MobileSidebar's
/// focus-on-open) with no IStringLocalizer&lt;LayoutResources&gt; dependency anywhere — every
/// label below is a plain hardcoded English string taken verbatim from LayoutResources.en-US.resx
/// at extraction time.
/// </summary>
public sealed class NavigationTests
{
    [Fact]
    public void DesktopSidebarRendersPrimaryNavigationAriaLabelBrandLinkAndAllFiveItems()
    {
        using var context = new BunitContext();

        var cut = context.Render<DesktopSidebar>();

        var aside = cut.Find("aside.desktop-sidebar");
        Assert.Equal("Primary navigation", aside.GetAttribute("aria-label"));

        var brandLink = cut.Find("a.desktop-sidebar__brand-link");
        Assert.Equal("beeday — go to Profile", brandLink.GetAttribute("aria-label"));

        foreach (var label in new[] { "Profile", "Daily", "Wallet", "Account", "Logout" })
        {
            Assert.Contains(label, cut.Markup);
        }
    }

    [Theory]
    [InlineData(false, "false", "Open navigation menu")]
    [InlineData(true, "true", "Close navigation menu")]
    public void MobileHeaderReflectsAriaExpandedAndAriaLabelForNavState(bool isOpen, string expectedAriaExpanded, string expectedAriaLabel)
    {
        using var context = new BunitContext();

        var cut = context.Render<MobileHeader>(parameters => parameters
            .Add(p => p.IsNavOpen, isOpen));

        var button = cut.Find("button.mobile-header__menu-button");
        Assert.Equal(expectedAriaExpanded, button.GetAttribute("aria-expanded"));
        Assert.Equal(expectedAriaLabel, button.GetAttribute("aria-label"));
        Assert.Equal("mobile-navigation", button.GetAttribute("aria-controls"));
    }

    private sealed class MobileHeaderHost : ComponentBase
    {
        private bool _isOpen;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<MobileHeader>(0);
            builder.AddAttribute(1, nameof(MobileHeader.IsNavOpen), _isOpen);
            builder.AddAttribute(2, nameof(MobileHeader.OnToggleNav), EventCallback.Factory.Create(this, Toggle));
            builder.CloseComponent();
        }

        private void Toggle() => _isOpen = !_isOpen;
    }

    [Fact]
    public void MobileHeaderReturnsFocusToTheTriggerButtonAfterItCloses()
    {
        // Sprint 30.20's real, valuable accessibility behavior: MobileHeader.OnAfterRenderAsync
        // moves focus back to its own toggle button once IsNavOpen transitions from true to
        // false. A host component supplies the state transition (the same way MainLayout does),
        // so the click goes through the real Blazor event pipeline on one persistent component
        // instance instead of creating a fresh one each time.
        using var context = new BunitContext();

        var cut = context.Render<MobileHeaderHost>();
        var button = cut.Find("button.mobile-header__menu-button");

        button.Click(); // closed -> open: no focus-return expected yet
        Assert.DoesNotContain(context.JSInterop.Invocations, i => i.Identifier == "Blazor._internal.domWrapper.focus");

        cut.Find("button.mobile-header__menu-button").Click(); // open -> closed: focus returns to the trigger
        Assert.Contains(context.JSInterop.Invocations, i => i.Identifier == "Blazor._internal.domWrapper.focus");
    }

    [Theory]
    [InlineData(false, "true")]
    [InlineData(true, "false")]
    public void MobileSidebarBackdropAndDrawerReflectIsOpenAndAriaHidden(bool isOpen, string expectedAriaHidden)
    {
        using var context = new BunitContext();

        var cut = context.Render<MobileSidebar>(parameters => parameters
            .Add(p => p.IsOpen, isOpen));

        var backdrop = cut.Find("div.mobile-nav-backdrop");
        var drawer = cut.Find("aside#mobile-navigation");

        Assert.Equal(isOpen, backdrop.ClassList.Contains("is-open"));
        Assert.Equal(isOpen, drawer.ClassList.Contains("is-open"));
        Assert.Equal(expectedAriaHidden, drawer.GetAttribute("aria-hidden"));
        Assert.Equal("Primary navigation", drawer.GetAttribute("aria-label"));
    }

    [Fact]
    public void MobileSidebarEscapeKeyInvokesOnClose()
    {
        using var context = new BunitContext();
        var closed = false;

        var cut = context.Render<MobileSidebar>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => closed = true)));

        cut.Find("aside#mobile-navigation").KeyDown("Escape");

        Assert.True(closed);
    }

    private sealed class MobileSidebarHost : ComponentBase
    {
        private bool _isOpen;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            // A real "open" trigger button, exactly like MainLayout's MobileHeader hookup — the test
            // interacts with it the same way a user would, going through the normal Blazor event
            // pipeline on one persistent component instance instead of poking internal test-only APIs.
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "type", "button");
            builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, Open));
            builder.AddContent(3, "Open");
            builder.CloseElement();

            builder.OpenComponent<MobileSidebar>(4);
            builder.AddAttribute(5, nameof(MobileSidebar.IsOpen), _isOpen);
            builder.AddAttribute(6, nameof(MobileSidebar.OnClose), EventCallback.Factory.Create(this, Close));
            builder.CloseComponent();
        }

        private void Open() => _isOpen = true;

        private void Close() => _isOpen = false;
    }

    [Fact]
    public void MobileSidebarMovesFocusToItsCloseButtonWhenItOpens()
    {
        // Sprint 21.3's focus-trap entry point: MobileSidebar.OnAfterRenderAsync moves focus to its
        // own close button the moment IsOpen transitions from false to true.
        using var context = new BunitContext();

        var cut = context.Render<MobileSidebarHost>();
        Assert.DoesNotContain(context.JSInterop.Invocations, i => i.Identifier == "Blazor._internal.domWrapper.focus");

        cut.Find("button").Click();

        Assert.Contains(context.JSInterop.Invocations, i => i.Identifier == "Blazor._internal.domWrapper.focus");
    }

    [Fact]
    public void NavigationItemRendersAsNavLinkWithAriaCurrentPageOnTheActiveRoute()
    {
        using var context = new BunitContext();
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/profile");

        var cut = context.Render<NavigationItem>(parameters => parameters
            .Add(p => p.Icon, BeeDayIconName.Profile)
            .Add(p => p.Label, "Profile")
            .Add(p => p.Href, "/profile"));

        var link = cut.Find("a");
        Assert.Equal("page", link.GetAttribute("aria-current"));
        Assert.Contains("is-active", link.ClassList);
    }

    [Fact]
    public void NavigationItemRendersAsNavLinkWithNoAriaCurrentOnAnInactiveRoute()
    {
        using var context = new BunitContext();
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/daily");

        var cut = context.Render<NavigationItem>(parameters => parameters
            .Add(p => p.Icon, BeeDayIconName.Profile)
            .Add(p => p.Label, "Profile")
            .Add(p => p.Href, "/profile"));

        var link = cut.Find("a");
        Assert.Null(link.GetAttribute("aria-current"));
        Assert.DoesNotContain("is-active", link.ClassList);
    }

    [Fact]
    public void NavigationItemRendersAsAButtonWithAriaLabelAndAriaExpandedWhenHrefIsNull()
    {
        using var context = new BunitContext();

        var cut = context.Render<NavigationItem>(parameters => parameters
            .Add(p => p.Icon, BeeDayIconName.Logout)
            .Add(p => p.Label, "Logout")
            .Add(p => p.AriaLabel, "Log out of beeday")
            .Add(p => p.AriaExpanded, true)
            .Add(p => p.ButtonType, "submit"));

        var button = cut.Find("button");
        Assert.Equal("submit", button.GetAttribute("type"));
        Assert.Equal("Log out of beeday", button.GetAttribute("aria-label"));
        Assert.Equal("true", button.GetAttribute("aria-expanded"));
        Assert.Contains("is-active", button.ClassList);
    }

    [Fact]
    public void NavigationItemsRendersHardcodedLabelsAndTheLogoutFormPostsToAuthLogout()
    {
        using var context = new BunitContext();

        var cut = context.Render<NavigationItems>();

        var nav = cut.Find("nav.navigation-items");
        Assert.Equal("Main navigation", nav.GetAttribute("aria-label"));

        foreach (var label in new[] { "Profile", "Daily", "Wallet", "Account", "Logout" })
        {
            Assert.Contains(label, cut.Markup);
        }

        var logoutForm = cut.Find("form.navigation-items__logout-form");
        Assert.Equal("post", logoutForm.GetAttribute("method"));
        Assert.Equal("/auth/logout", logoutForm.GetAttribute("action"));
    }
}
