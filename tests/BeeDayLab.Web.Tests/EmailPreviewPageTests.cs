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

    [Fact]
    public void QuickReviewOffersAllFourRealTemplateLocaleCombinationsAsDirectLinks()
    {
        using var context = CreateContext();

        var cut = context.Render<EmailPreviewPage>();
        var links = cut.Find("[data-testid='email-quick-review']").QuerySelectorAll("a");

        Assert.Equal(4, links.Length);
        Assert.Contains(links, a => a.GetAttribute("href") == "/emails?template=confirmation&culture=pt-BR&width=standard");
        Assert.Contains(links, a => a.GetAttribute("href") == "/emails?template=confirmation&culture=en-US&width=standard");
        Assert.Contains(links, a => a.GetAttribute("href") == "/emails?template=reset&culture=pt-BR&width=standard");
        Assert.Contains(links, a => a.GetAttribute("href") == "/emails?template=reset&culture=en-US&width=standard");
    }

    [Fact]
    public void EnvelopeReviewChromeShowsFromToSubjectSeparatelyFromTheEmailBodyItself()
    {
        using var context = CreateContext();

        var cut = context.Render<EmailPreviewPage>();
        var envelope = cut.Find("[data-testid='email-envelope']");

        var from = cut.Find("[data-testid='email-from']").TextContent;
        var to = cut.Find("[data-testid='email-to']").TextContent;
        Assert.Contains("beeday-lab.invalid", from, StringComparison.Ordinal);
        Assert.Contains("beeday-lab.invalid", to, StringComparison.Ordinal);

        // The envelope chrome (From/To) must never leak into the actual composed email HTML — it is
        // Lab-only review chrome living outside the iframe, not part of _preview.Html/PlainText.
        var srcdoc = cut.Find("[data-testid='email-frame']").GetAttribute("srcdoc");
        Assert.DoesNotContain(from, srcdoc, StringComparison.Ordinal);
        Assert.NotNull(envelope);
    }

    [Theory]
    [InlineData(null, null, "/emails/confirmation/rendered?culture=en-US")]
    [InlineData("confirmation", "pt-BR", "/emails/confirmation/rendered?culture=pt-BR")]
    [InlineData("reset", "en-US", "/emails/password-reset/rendered?culture=en-US")]
    [InlineData("reset", "pt-BR", "/emails/password-reset/rendered?culture=pt-BR")]
    public void OpenFullPreviewLinkPointsAtTheStandaloneRenderedRouteForTheCurrentSelection(
        string? template, string? culture, string expectedHref)
    {
        using var context = CreateContext();
        if (template is not null)
        {
            Navigate(context, template, culture!, "standard");
        }

        var cut = context.Render<EmailPreviewPage>();
        var link = cut.Find("[data-testid='email-open-full-preview']");

        Assert.Equal(expectedHref, link.GetAttribute("href"));
        Assert.Equal("_blank", link.GetAttribute("target"));
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
