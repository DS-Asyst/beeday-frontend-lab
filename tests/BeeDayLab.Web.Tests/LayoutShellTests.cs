using BeeDayLab.Web.Components.DesignSystem.Feedback;
using BeeDayLab.Web.Components.Layout;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Deterministic bUnit component tests for Sprint 33.9 (FE33-039..042): proves the four Layout
/// shells (MainLayout, OnboardingLayout, PublicLayout, EditorialLayout) render their full
/// structure — including the composed navigation/footer chrome components they own — without
/// needing any production service (AuthenticatedUserInitializer, real auth, EF Core). MainLayout
/// in particular used to inject AuthenticatedUserInitializer and call EnsureInitializedAsync() in
/// OnInitializedAsync in the BeeDay source; this Sprint drops both, so simply rendering it with no
/// such service registered anywhere in the bUnit DI container is itself proof the dependency is
/// gone.
/// </summary>
public sealed class LayoutShellTests
{
    [Fact]
    public void MainLayoutRendersFullShellStructureAroundBodyWithNoAuthInitializer()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<ToastService>();

        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(p => p.Body, (RenderFragment)(builder => builder.AddContent(0, "Page content"))));

        Assert.NotNull(cut.Find("div.beeday-app"));
        Assert.NotNull(cut.Find("div.beeday-shell"));
        Assert.NotNull(cut.Find("aside.desktop-sidebar"));
        Assert.NotNull(cut.Find("header.mobile-header"));
        Assert.NotNull(cut.Find("aside#mobile-navigation"));
        Assert.NotNull(cut.Find("main#main-content"));
        Assert.Contains("Page content", cut.Markup);
        Assert.NotNull(cut.Find("div.beeday-toast-region"));
    }

    [Fact]
    public void MainLayoutMobileNavToggleOpensAndClosesTheDrawer()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<ToastService>();

        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(p => p.Body, (RenderFragment)(builder => builder.AddContent(0, "Body"))));

        var drawer = cut.Find("aside#mobile-navigation");
        Assert.DoesNotContain("is-open", drawer.ClassList);
        Assert.Equal("true", drawer.GetAttribute("aria-hidden"));

        cut.Find("button.mobile-header__menu-button").Click();

        drawer = cut.Find("aside#mobile-navigation");
        Assert.Contains("is-open", drawer.ClassList);
        Assert.Equal("false", drawer.GetAttribute("aria-hidden"));

        cut.Find("button.mobile-header__menu-button").Click();

        drawer = cut.Find("aside#mobile-navigation");
        Assert.DoesNotContain("is-open", drawer.ClassList);
        Assert.Equal("true", drawer.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void OnboardingLayoutRendersBodyInsideMainAndToastHost()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<ToastService>();

        var cut = context.Render<OnboardingLayout>(parameters => parameters
            .Add(p => p.Body, (RenderFragment)(builder => builder.AddContent(0, "Onboarding step"))));

        Assert.NotNull(cut.Find("div.onboarding-layout"));
        Assert.Contains("Onboarding step", cut.Find("main").TextContent);
        Assert.NotNull(cut.Find("div.beeday-toast-region"));
    }

    [Fact]
    public void PublicLayoutRendersHeaderMainFooterAndToastHost()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<ToastService>();

        var cut = context.Render<PublicLayout>(parameters => parameters
            .Add(p => p.Body, (RenderFragment)(builder => builder.AddContent(0, "Public page"))));

        Assert.NotNull(cut.Find("div.public-layout"));
        Assert.NotNull(cut.Find("header.public-header"));
        var main = cut.Find("main#main-content");
        Assert.Contains("public-layout__main", main.ClassList);
        Assert.Contains("Public page", main.TextContent);
        Assert.NotNull(cut.Find("footer.app-footer"));
        Assert.NotNull(cut.Find("div.beeday-toast-region"));
    }

    [Fact]
    public void EditorialLayoutRendersMainAndEditorialFooter()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<ToastService>();

        var cut = context.Render<EditorialLayout>(parameters => parameters
            .Add(p => p.Body, (RenderFragment)(builder => builder.AddContent(0, "Editorial page"))));

        var main = cut.Find("main.editorial-layout__main");
        Assert.Contains("Editorial page", main.TextContent);
        Assert.NotNull(cut.Find("footer.editorial-footer"));
        Assert.NotNull(cut.Find("div.beeday-toast-region"));

        // EditorialLayout has no separate PublicHeader/AppFooter — brand + nav render as part of the
        // page's own hero elsewhere, and AppFooter is replaced entirely by EditorialFooter (Sprint 29.4).
        Assert.Empty(cut.FindAll("header.public-header"));
        Assert.Empty(cut.FindAll("footer.app-footer"));
    }
}
