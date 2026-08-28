using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using EmailPreviewPage = BeeDayLab.Web.Components.Pages.Emails.EmailPreview;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Sprint 33.15 (FE33-101/102/103): every mapped e-mail state (2 templates × 2 locales × 2 widths)
/// must be directly, deep-link selectable, and the preview surface must never send anything or
/// depend on a real credential/provider.
/// </summary>
public sealed class EmailPreviewPageTests
{
    [Fact]
    public void DefaultRoute_PreviewsTheConfirmationTemplateInEnglishAtStandardWidth()
    {
        using var context = CreateContext();

        var cut = context.Render<EmailPreviewPage>();

        Assert.Equal("Confirm your beeday email", cut.Find("[data-testid='email-subject']").TextContent);
        Assert.Contains("max-width:640px", cut.Find(".email-preview-page__frame-wrapper").GetAttribute("style"), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("confirmation", "en-US", "narrow", "Confirm your beeday email", 375)]
    [InlineData("confirmation", "pt-BR", "standard", "Confirme seu e-mail beeday", 640)]
    [InlineData("reset", "en-US", "standard", "Reset your beeday password", 640)]
    [InlineData("reset", "pt-BR", "narrow", "Redefina sua senha beeday", 375)]
    public void EveryMappedTemplateLocaleWidthCombinationIsDirectlySelectable(
        string template, string culture, string width, string expectedSubject, int expectedPixels)
    {
        using var context = CreateContext();
        Navigate(context, template, culture, width);

        var cut = context.Render<EmailPreviewPage>();

        Assert.Equal(expectedSubject, cut.Find("[data-testid='email-subject']").TextContent);
        Assert.Contains($"max-width:{expectedPixels}px", cut.Find(".email-preview-page__frame-wrapper").GetAttribute("style"), StringComparison.Ordinal);

        var srcdoc = cut.Find("[data-testid='email-frame']").GetAttribute("srcdoc");
        Assert.NotNull(srcdoc);
        Assert.Contains("beeday-lab.invalid", srcdoc, StringComparison.Ordinal);
        Assert.DoesNotContain("beeday.app", srcdoc, StringComparison.OrdinalIgnoreCase);

        var plainText = cut.Find("[data-testid='email-plain-text']").TextContent;
        Assert.Contains("beeday-lab.invalid", plainText, StringComparison.Ordinal);
    }

    [Fact]
    public void PageNeverContainsASendControlOrRealProviderCredential()
    {
        using var context = CreateContext();

        var cut = context.Render<EmailPreviewPage>();
        var markup = cut.Markup;

        Assert.DoesNotContain("resend", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("smtp", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("api-key", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(">Send<", markup, StringComparison.OrdinalIgnoreCase);
    }

    private static void Navigate(BunitContext context, string template, string culture, string width)
    {
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo($"{navigation.BaseUri}emails?template={template}&culture={culture}&width={width}");
    }

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddLocalization();
        return context;
    }
}
