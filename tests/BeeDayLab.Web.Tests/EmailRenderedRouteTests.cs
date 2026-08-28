using BeeDayLab.Web.Emails;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Owner review requirement (Sprint 33.18-R, after visual approval of the CSS composition fix):
/// standalone routes must serve the exact raw HTML body an end user would receive, with nothing
/// else on the page. These endpoints are mapped as minimal API routes in Program.cs — not
/// <c>@page</c> components — specifically so they bypass the Blazor render pipeline (and therefore
/// the Lab's App.razor shell/layout/CSS) entirely; every assertion here goes through a real HTTP
/// round-trip via <see cref="WebApplicationFactory{TEntryPoint}"/>, the same pattern
/// <c>LoginAndAuthTests</c> established in Sprint 33.12 for testing a Program.cs-mapped route.
/// </summary>
public sealed class EmailRenderedRouteTests
{
    [Theory]
    [InlineData("/emails/confirmation/rendered", "en-US", TransactionalEmailKind.Confirmation)]
    [InlineData("/emails/confirmation/rendered", "pt-BR", TransactionalEmailKind.Confirmation)]
    [InlineData("/emails/password-reset/rendered", "en-US", TransactionalEmailKind.PasswordReset)]
    [InlineData("/emails/password-reset/rendered", "pt-BR", TransactionalEmailKind.PasswordReset)]
    public async Task StandaloneRouteServesTheExactHtmlTheCatalogComposesForEveryKindAndLocale(
        string path, string culture, TransactionalEmailKind kind)
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync($"{path}?culture={culture}", TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(TransactionalEmailTemplateCatalog.Compose(kind, culture).Html, body);
    }

    [Fact]
    public async Task StandaloneRouteDefaultsToEnglishWhenNoCultureIsSupplied()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/emails/confirmation/rendered", TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(TransactionalEmailTemplateCatalog.Compose(TransactionalEmailKind.Confirmation, "en-US").Html, body);
    }

    [Theory]
    [InlineData("/emails/confirmation/rendered")]
    [InlineData("/emails/password-reset/rendered")]
    public async Task StandaloneRouteNeverIncludesTheLabShellOrNavigation(string path)
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var body = await client.GetStringAsync($"{path}?culture=en-US", TestContext.Current.CancellationToken);

        // Anything below would only be present if the response went through the normal Blazor
        // Web App host document (App.razor) instead of being served as a bare minimal API response.
        Assert.DoesNotContain("blazor.web.js", body, StringComparison.Ordinal);
        Assert.DoesNotContain("beeday-app", body, StringComparison.Ordinal);
        Assert.DoesNotContain("email-preview-page", body, StringComparison.Ordinal);
        Assert.DoesNotContain("gallery-page", body, StringComparison.Ordinal);
        Assert.DoesNotContain("preview-page", body, StringComparison.Ordinal);
        Assert.StartsWith("<!doctype html>", body, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/emails/confirmation/rendered")]
    [InlineData("/emails/password-reset/rendered")]
    public async Task StandaloneRouteActionUrlAlwaysUsesTheReservedInvalidHost(string path)
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var body = await client.GetStringAsync($"{path}?culture=en-US", TestContext.Current.CancellationToken);

        Assert.Contains("beeday-lab.invalid", body, StringComparison.Ordinal);
        Assert.DoesNotContain("beeday.app", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExactlyTheTwoRealProductionTransactionalEmailKindsExist()
    {
        // Owner boundary: "no extra fake transactional template was invented". BeeDay's
        // IdentityEmailComposer exposes exactly ComposeEmailConfirmation/ComposePasswordReset.
        var kinds = Enum.GetValues<TransactionalEmailKind>();

        Assert.Equal(2, kinds.Length);
        Assert.Contains(TransactionalEmailKind.Confirmation, kinds);
        Assert.Contains(TransactionalEmailKind.PasswordReset, kinds);
    }
}
