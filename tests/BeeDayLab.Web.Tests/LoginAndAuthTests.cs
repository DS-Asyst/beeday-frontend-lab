using System.Net;
using System.Reflection;
using AngleSharp;
using BeeDayLab.Web.Components.Pages.Identity;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Sprint 33.12 (FE33-077, FE33-087) tests for Login.razor's query-string-driven feedback banners
/// and the new Lab-local POST /auth/login minimal API in Program.cs. The endpoint test uses
/// <see cref="WebApplicationFactory{TEntryPoint}"/> against the top-level <c>Program</c> class
/// (exposed to this assembly via the existing InternalsVisibleTo in BeeDayLab.Web.csproj) — there was
/// no prior precedent in this repo for testing a Program.cs-mapped route directly, so this is the
/// first; every assertion goes through a real HTTP round-trip rather than reimplementing the
/// endpoint's logic, so the test would actually fail if the real route regressed.
///
/// Because /auth/login binds from form data, ASP.NET Core automatically requires antiforgery
/// validation for it (the same mechanism that already protects /culture/set) — a bare POST gets
/// rejected 400 before reaching the handler. Each endpoint test first GETs /login (which renders the
/// same &lt;AntiforgeryToken /&gt; production markup uses) to obtain a real antiforgery cookie +
/// hidden field pair, then submits that pair alongside the credential fields, exactly like a real
/// browser submitting the rendered form would.
/// </summary>
public sealed class LoginAndAuthTests
{
    [Fact]
    public void LoginIsRoutedAtLoginPath()
    {
        var routes = typeof(Login).GetCustomAttributes<RouteAttribute>(inherit: false);

        Assert.Contains(routes, r => r.Template == "/login");
    }

    [Fact]
    public void LoginRendersInvalidCredentialsBannerWhenErrorQueryStringIsInvalid()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        context.Services.AddLocalization();
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("error", "invalid"));

        var cut = context.Render<Login>();

        Assert.NotNull(cut.Find(".auth-feedback--error"));
    }

    [Fact]
    public void LoginRendersConfirmedSuccessBannerWhenConfirmedQueryStringIsTrue()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        context.Services.AddLocalization();
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("confirmed", true));

        var cut = context.Render<Login>();

        Assert.NotNull(cut.Find(".auth-feedback--success"));
    }

    [Fact]
    public void LoginRendersNoFeedbackBannersByDefault()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        context.Services.AddLocalization();

        var cut = context.Render<Login>();

        Assert.Empty(cut.FindAll(".auth-feedback"));
    }

    [Fact]
    public void LoginFormPostsToTheLabLocalAuthLoginEndpoint()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        context.Services.AddLocalization();

        var cut = context.Render<Login>();

        var form = cut.Find("form.auth-form");
        Assert.Equal("/auth/login", form.GetAttribute("action"));
        Assert.Equal("post", form.GetAttribute("method"));
    }

    [Fact]
    public async Task AuthLoginRedirectsToReturnUrlOnDemoCredentialMatch()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var response = await PostLoginAsync(client, "demo@beeday.app", "BeeDayLab!2026", "/account");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/account", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task AuthLoginRedirectsToProfileCreateFallbackWhenNoReturnUrlGiven()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var response = await PostLoginAsync(client, "demo@beeday.app", "BeeDayLab!2026", returnUrl: null);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/profile/create", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task AuthLoginRedirectsToErrorInvalidOnWrongPassword()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var response = await PostLoginAsync(client, "demo@beeday.app", "wrong-password", returnUrl: null);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/login?error=invalid", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task AuthLoginRedirectsToErrorInvalidOnUnknownEmail()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var response = await PostLoginAsync(client, "someone-else@example.com", "BeeDayLab!2026", returnUrl: null);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/login?error=invalid", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task AuthLoginResponseNeverSetsACookie()
    {
        // Boundary check: the Sprint 33.12 brief requires the endpoint itself set NO cookie and NO
        // session, unlike production's real POST /auth/login — unlike the antiforgery cookie set by
        // the earlier GET /login, the POST /auth/login response itself must carry none.
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var response = await PostLoginAsync(client, "demo@beeday.app", "BeeDayLab!2026", returnUrl: null);

        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    [Theory]
    [InlineData("//evil.example.com")]
    [InlineData("/\\evil.example.com")]
    [InlineData("https://evil.example.com")]
    public async Task AuthLoginIgnoresAnOpenRedirectReturnUrlAndFallsBackToProfileCreate(string maliciousReturnUrl)
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var response = await PostLoginAsync(client, "demo@beeday.app", "BeeDayLab!2026", maliciousReturnUrl);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/profile/create", response.Headers.Location?.OriginalString);
    }

    private static async Task<HttpResponseMessage> PostLoginAsync(HttpClient client, string email, string password, string? returnUrl)
    {
        var token = await GetAntiforgeryTokenAsync(client, "/login");

        var formData = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["email"] = email,
            ["password"] = password
        };

        if (returnUrl is not null)
        {
            formData["returnUrl"] = returnUrl;
        }

        return await client.PostAsync("/auth/login", new FormUrlEncodedContent(formData));
    }

    private static async Task<string> GetAntiforgeryTokenAsync(HttpClient client, string pageUrl)
    {
        var html = await client.GetStringAsync(pageUrl);
        var browsingContext = BrowsingContext.New(Configuration.Default);
        var document = await browsingContext.OpenAsync(request => request.Content(html));
        var input = document.QuerySelector("input[name='__RequestVerificationToken']")
            ?? throw new InvalidOperationException($"No antiforgery token found on {pageUrl}.");

        return input.GetAttribute("value") ?? throw new InvalidOperationException("Antiforgery token input has no value.");
    }
}
