using System.Reflection;
using BeeDayLab.Web.Components.DesignSystem.Feedback;
using BeeDayLab.Web.Components.Pages.Identity;
using BeeDayLab.Web.Scenarios;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Sprint 33.12 (FE33-086) tests for Account.razor — the biggest single item in this Sprint. Covers:
/// routing at both "/account" and "/settings", scenario-driven initial load, each of the three
/// independent sections' own busy/save/scenario-outcome flow (Profile/Security/Preferences do not
/// share a single busy flag), and that the real "/culture/set" JS-interop flow is still invoked when
/// the preferred language actually changes.
///
/// Every wait predicate below asserts on "#account-name" (inside the Profile section, only rendered
/// once _isLoading flips false) rather than a generic "form" count — the always-present hidden
/// culture-sync-form renders unconditionally from the very first render, so waiting on "any form
/// exists" would return before the real, scenario-loaded content ever appears.
/// </summary>
public sealed class AccountPageTests
{
    [Fact]
    public void AccountIsRoutedAtBothAccountAndSettingsPaths()
    {
        var routes = typeof(Account).GetCustomAttributes<RouteAttribute>(inherit: false)
            .Select(r => r.Template)
            .ToList();

        Assert.Contains("/account", routes);
        Assert.Contains("/settings", routes);
    }

    [Fact]
    public void AccountLoadsSyntheticProfileDataFromTheScenarioProvider()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        RegisterServices(context, ScenarioState.Populated);

        var cut = context.Render<Account>();
        WaitForLoaded(cut);

        var nameInput = cut.Find("#account-name");
        Assert.Equal("Jordan Silva", nameInput.GetAttribute("value"));
    }

    [Fact]
    public void SaveProfileShowsSuccessToastWhenScenarioSucceeds()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        RegisterServices(context, ScenarioState.Populated);
        var toast = context.Services.GetRequiredService<ToastService>();

        var cut = context.Render<Account>();
        WaitForLoaded(cut);

        // Forms render in DOM order Profile/Security/Preferences (matching Account.razor's markup),
        // and BeeDaySettingsForm's "FormName" is a captured AdditionalAttributes value, not the HTML
        // "name" attribute, so index-based selection (excluding the always-present hidden
        // culture-sync-form, which comes after all three) is the stable option here.
        cut.FindAll("form")[0].Submit();

        cut.WaitForAssertion(() => Assert.NotEmpty(toast.Messages), TimeSpan.FromSeconds(3));
        Assert.Equal(ToastVariant.Success, toast.Messages[^1].Variant);
    }

    [Fact]
    public void SaveProfileShowsErrorToastWhenScenarioStateIsError()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        RegisterServices(context, ScenarioState.Error);
        var toast = context.Services.GetRequiredService<ToastService>();

        var cut = context.Render<Account>();
        WaitForLoaded(cut);

        cut.FindAll("form")[0].Submit();

        cut.WaitForAssertion(() => Assert.NotEmpty(toast.Messages), TimeSpan.FromSeconds(3));
        Assert.Equal(ToastVariant.Error, toast.Messages[^1].Variant);
    }

    [Fact]
    public void ChangePasswordShowsSuccessToastIndependentlyOfProfileSection()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        RegisterServices(context, ScenarioState.Populated);
        var toast = context.Services.GetRequiredService<ToastService>();

        var cut = context.Render<Account>();
        WaitForLoaded(cut);

        cut.Find("#current-password").Change("OldPassword1");
        cut.Find("#new-password").Change("NewPassword1");
        cut.Find("#confirm-password").Change("NewPassword1");
        cut.FindAll("form")[1].Submit();

        cut.WaitForAssertion(() => Assert.NotEmpty(toast.Messages), TimeSpan.FromSeconds(3));
        Assert.Equal(ToastVariant.Success, toast.Messages[^1].Variant);
    }

    [Fact]
    public void SavePreferencesShowsSuccessToastWhenLanguageIsUnchanged()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        RegisterServices(context, ScenarioState.Populated);
        var toast = context.Services.GetRequiredService<ToastService>();

        var cut = context.Render<Account>();
        WaitForLoaded(cut);

        // Only the theme changes here, not the language — SavePreferencesAsync should show a toast
        // rather than trigger the real culture-sync/reload flow.
        cut.Find("#account-theme").Change("Dark");
        cut.FindAll("form")[2].Submit();

        cut.WaitForAssertion(() => Assert.NotEmpty(toast.Messages), TimeSpan.FromSeconds(3));
        Assert.Equal(ToastVariant.Success, toast.Messages[^1].Variant);
    }

    [Fact]
    public void SavePreferencesInvokesTheRealCultureSyncModuleWhenLanguageChanges()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        RegisterServices(context, ScenarioState.Populated);
        var module = context.JSInterop.SetupModule("./js/beeday-culture-sync.js");
        module.SetupVoid("submitCultureSync", _ => true).SetVoidResult();

        var cut = context.Render<Account>();
        WaitForLoaded(cut);

        cut.Find("#account-language").Change("Portuguese");
        cut.FindAll("form")[2].Submit();

        // No JSException means the exact configured module path ("./js/beeday-culture-sync.js", no
        // "?v=..." suffix) was the one actually imported and "submitCultureSync" was invoked — the
        // Account.razor.cs equivalent of ModalAndSortableTests' own JS-interop-path proof.
        cut.WaitForAssertion(() => module.VerifyInvoke("submitCultureSync"), TimeSpan.FromSeconds(3));
    }

    private static void WaitForLoaded(IRenderedComponent<Account> cut) =>
        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll("#account-name")), TimeSpan.FromSeconds(3));

    private static void RegisterServices(BunitContext context, ScenarioState state)
    {
        context.Services.AddLocalization();
        context.Services.AddScoped<ToastService>();
        context.Services.AddScoped(_ => new ScenarioSelection { State = state });
        context.Services.AddSingleton<AccountScenarioProvider>();
    }
}
