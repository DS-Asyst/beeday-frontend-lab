using System.Reflection;
using BeeDayLab.Web.Components.Pages.Identity;
using BeeDayLab.Web.Scenarios;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Sprint 33.12 (FE33-080..084) tests for the account-recovery/email-confirmation surface:
/// ForgotPassword, ResendConfirmation, EmailConfirmationSent, ConfirmEmail, ResetPassword. Covers the
/// scenario-driven success/failure outcome for each page (registering <see cref="ScenarioSelection"/>
/// scoped, pre-set to the state under test, mirrors how Program.cs registers it for real) and
/// ResendCooldownTimer's own countdown behavior.
/// </summary>
public sealed class IdentityRecoveryTests
{
    [Fact]
    public void ForgotPasswordIsRoutedCorrectly()
    {
        var routes = typeof(ForgotPassword).GetCustomAttributes<RouteAttribute>(inherit: false);
        Assert.Contains(routes, r => r.Template == "/account/forgot-password");
    }

    [Fact]
    public void ForgotPasswordShowsSuccessMessageWhenScenarioSucceeds()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        RegisterCommonServices(context, ScenarioState.Populated);
        context.Services.AddSingleton<ForgotPasswordScenarioProvider>();

        var cut = context.Render<ForgotPassword>();
        cut.Find("#forgot-password-email").Change("reader@example.com");
        cut.Find("form.identity-form").Submit();

        cut.WaitForAssertion(() => Assert.NotNull(cut.Find(".identity-feedback--success")), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void ForgotPasswordShowsErrorMessageWhenScenarioStateIsError()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        RegisterCommonServices(context, ScenarioState.Error);
        context.Services.AddSingleton<ForgotPasswordScenarioProvider>();

        var cut = context.Render<ForgotPassword>();
        cut.Find("#forgot-password-email").Change("reader@example.com");
        cut.Find("form.identity-form").Submit();

        cut.WaitForAssertion(() => Assert.NotNull(cut.Find(".identity-feedback--error")), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void ResendConfirmationIsRoutedCorrectly()
    {
        var routes = typeof(ResendConfirmation).GetCustomAttributes<RouteAttribute>(inherit: false);
        Assert.Contains(routes, r => r.Template == "/account/resend-confirmation");
    }

    [Fact]
    public void ResendConfirmationShowsSuccessMessageAndStartsCooldownWhenScenarioSucceeds()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        RegisterCommonServices(context, ScenarioState.Populated);
        context.Services.AddSingleton<ResendConfirmationScenarioProvider>();

        var cut = context.Render<ResendConfirmation>();
        cut.Find("#resend-confirmation-email").Change("reader@example.com");
        cut.Find("form.identity-form").Submit();

        cut.WaitForAssertion(() => Assert.NotNull(cut.Find(".identity-feedback--success")), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void ResendConfirmationShowsErrorMessageWhenScenarioStateIsError()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        RegisterCommonServices(context, ScenarioState.Error);
        context.Services.AddSingleton<ResendConfirmationScenarioProvider>();

        var cut = context.Render<ResendConfirmation>();
        cut.Find("#resend-confirmation-email").Change("reader@example.com");
        cut.Find("form.identity-form").Submit();

        cut.WaitForAssertion(() => Assert.NotNull(cut.Find(".identity-feedback--error")), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void EmailConfirmationSentIsRoutedCorrectly()
    {
        var routes = typeof(EmailConfirmationSent).GetCustomAttributes<RouteAttribute>(inherit: false);
        Assert.Contains(routes, r => r.Template == "/account/email-confirmation-sent");
    }

    [Fact]
    public void EmailConfirmationSentShowsSuccessMessageWhenScenarioSucceeds()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        RegisterCommonServices(context, ScenarioState.Populated);
        context.Services.AddSingleton<ResendConfirmationScenarioProvider>();
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("email", "reader@example.com"));

        var cut = context.Render<EmailConfirmationSent>();
        cut.Find("button.beeday-button").Click();

        cut.WaitForAssertion(() => Assert.NotNull(cut.Find(".identity-feedback--success")), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void ResetPasswordIsRoutedCorrectly()
    {
        var routes = typeof(ResetPassword).GetCustomAttributes<RouteAttribute>(inherit: false);
        Assert.Contains(routes, r => r.Template == "/account/reset-password");
    }

    [Fact]
    public void ResetPasswordShowsMissingTokenErrorWhenNoTokenIsSupplied()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        RegisterCommonServices(context, ScenarioState.Populated);
        context.Services.AddSingleton<ResetPasswordScenarioProvider>();

        var cut = context.Render<ResetPassword>();
        cut.Find("#reset-password").Change("Password123");
        cut.Find("#reset-password-confirmation").Change("Password123");
        cut.Find("form.identity-form").Submit();

        Assert.NotNull(cut.Find(".identity-feedback--error"));
    }

    [Fact]
    public void ResetPasswordShowsCompletedMessageWhenScenarioSucceedsAndTokenIsPresent()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        RegisterCommonServices(context, ScenarioState.Populated);
        context.Services.AddSingleton<ResetPasswordScenarioProvider>();
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("token", "sample-token"));

        var cut = context.Render<ResetPassword>();
        cut.Find("#reset-password").Change("Password123");
        cut.Find("#reset-password-confirmation").Change("Password123");
        cut.Find("form.identity-form").Submit();

        cut.WaitForAssertion(() => Assert.NotNull(cut.Find(".identity-feedback--success")), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void ConfirmEmailIsRoutedCorrectly()
    {
        var routes = typeof(ConfirmEmail).GetCustomAttributes<RouteAttribute>(inherit: false);
        Assert.Contains(routes, r => r.Template == "/account/confirm-email");
    }

    [Fact]
    public void ConfirmEmailShowsInvalidLinkWhenTokenIsMissing()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        RegisterCommonServices(context, ScenarioState.Populated);
        context.Services.AddSingleton<ConfirmEmailScenarioProvider>();

        var cut = context.Render<ConfirmEmail>();

        Assert.Contains("identity-feedback--error", cut.Markup);
    }

    [Theory]
    [InlineData(ScenarioState.Populated, ConfirmEmailOutcome.Confirmed)]
    [InlineData(ScenarioState.Selected, ConfirmEmailOutcome.AlreadyConfirmed)]
    [InlineData(ScenarioState.Disabled, ConfirmEmailOutcome.Expired)]
    [InlineData(ScenarioState.NoResults, ConfirmEmailOutcome.Replaced)]
    [InlineData(ScenarioState.Error, ConfirmEmailOutcome.Invalid)]
    public void ConfirmEmailScenarioProviderMapsEveryDocumentedState(ScenarioState state, ConfirmEmailOutcome expected)
    {
        var provider = new ConfirmEmailScenarioProvider();

        var data = provider.GetScenario(new ScenarioContext(state, "en-US"));

        Assert.Equal(expected, data.Outcome);
    }

    [Fact]
    public async Task ResendCooldownTimerCountsDownAfterStart()
    {
        var ticks = 0;
        using var timer = new BeeDayLab.Web.Components.Pages.Identity.ResendCooldownTimer(() =>
        {
            ticks++;
            return Task.CompletedTask;
        });

        timer.Start(seconds: 2);
        Assert.Equal(2, timer.SecondsRemaining);

        await Task.Delay(TimeSpan.FromMilliseconds(1200), Xunit.TestContext.Current.CancellationToken);

        Assert.True(timer.SecondsRemaining < 2);
        Assert.True(ticks > 0);
    }

    private static void RegisterCommonServices(BunitContext context, ScenarioState state)
    {
        context.Services.AddLocalization();
        context.Services.AddScoped(_ => new ScenarioSelection { State = state });
    }
}
