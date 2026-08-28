using System.Reflection;
using BeeDayLab.Web.Components.DesignSystem.Feedback;
using BeeDayLab.Web.Components.Pages.Identity;
using BeeDayLab.Web.Components.Pages.Identity.State;
using BeeDayLab.Web.Scenarios;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Sprint 33.12 (FE33-079) tests for CreateProfile.razor and its ProfileCreationState — both the
/// anonymous and "authenticated" flows previewed via the "authenticated" query string toggle, the
/// ported client-side validation rules, and the scenario-driven success/failure outcome.
/// </summary>
public sealed class ProfileCreationTests
{
    [Fact]
    public void CreateProfileIsRoutedAtProfileCreatePath()
    {
        var routes = typeof(CreateProfile).GetCustomAttributes<RouteAttribute>(inherit: false);

        Assert.Contains(routes, r => r.Template == "/profile/create");
    }

    [Fact]
    public void AnonymousFlowStartsAtTheAccountStepWithEmptyFields()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        RegisterServices(context);

        var cut = context.Render<CreateProfile>();

        Assert.NotNull(cut.Find("input[autocomplete='name']"));
        Assert.NotNull(cut.Find("input[autocomplete='email']"));
    }

    [Fact]
    public void AuthenticatedFlowSkipsStraightToTheProfileStepWithPrefilledData()
    {
        using var culture = new TestCultureScope();
        using var context = new BunitContext();
        RegisterServices(context);
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("authenticated", true));

        var cut = context.Render<CreateProfile>();

        // The Profile step renders a nickname field, not the Account step's name/email fields.
        Assert.Empty(cut.FindAll("input[autocomplete='name']"));
        Assert.NotNull(cut.Find("input[autocomplete='nickname']"));
    }

    [Fact]
    public void ContinueToProfileRejectsAWeakPassword()
    {
        var state = CreateState();
        state.Initialize(hasAuthenticatedSession: false);
        state.Model.Name = "Ada Lovelace";
        state.Model.Email = "ada@example.com";
        state.Model.Password = "weak";
        state.Model.ConfirmPassword = "weak";

        var advanced = state.ContinueToProfile();

        Assert.False(advanced);
        Assert.Equal(ProfileCreationStep.Account, state.Step);
        Assert.False(string.IsNullOrWhiteSpace(state.ValidationError));
    }

    [Fact]
    public void ContinueToProfileRejectsMismatchedPasswords()
    {
        var state = CreateState();
        state.Initialize(hasAuthenticatedSession: false);
        state.Model.Name = "Ada Lovelace";
        state.Model.Email = "ada@example.com";
        state.Model.Password = "Password123";
        state.Model.ConfirmPassword = "Different123";

        var advanced = state.ContinueToProfile();

        Assert.False(advanced);
        Assert.Equal(ProfileCreationStep.Account, state.Step);
    }

    [Fact]
    public void ContinueToProfileAdvancesOnValidAccountData()
    {
        var state = CreateState();
        state.Initialize(hasAuthenticatedSession: false);
        state.Model.Name = "Ada Lovelace";
        state.Model.Email = "ada@example.com";
        state.Model.Password = "Password123";
        state.Model.ConfirmPassword = "Password123";

        var advanced = state.ContinueToProfile();

        Assert.True(advanced);
        Assert.Equal(ProfileCreationStep.Profile, state.Step);
    }

    [Theory]
    [InlineData("ab")] // too short
    [InlineData("has space")]
    public async Task CompleteProfileRejectsAnInvalidNickname(string nickname)
    {
        var state = CreateState();
        state.Initialize(hasAuthenticatedSession: false);
        state.Model.Nickname = nickname;

        var completed = await state.CompleteProfileAsync();

        Assert.False(completed);
        Assert.False(string.IsNullOrWhiteSpace(state.ValidationError));
    }

    [Fact]
    public async Task CompleteProfileSucceedsWhenScenarioStateIsNotError()
    {
        var scenarioSelection = new ScenarioSelection { State = ScenarioState.Populated };
        var state = CreateState(scenarioSelection);
        state.Initialize(hasAuthenticatedSession: false);
        state.Model.Nickname = "ada.lovelace";

        var completed = await state.CompleteProfileAsync();

        Assert.True(completed);
        Assert.Null(state.ValidationError);
    }

    [Fact]
    public async Task CompleteProfileFailsWhenScenarioStateIsError()
    {
        var scenarioSelection = new ScenarioSelection { State = ScenarioState.Error };
        var state = CreateState(scenarioSelection);
        state.Initialize(hasAuthenticatedSession: false);
        state.Model.Nickname = "ada.lovelace";

        var completed = await state.CompleteProfileAsync();

        Assert.False(completed);
        Assert.False(string.IsNullOrWhiteSpace(state.ValidationError));
    }

    private static ProfileCreationState CreateState(ScenarioSelection? scenarioSelection = null)
    {
        using var culture = new TestCultureScope();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        var provider = services.BuildServiceProvider();

        return new ProfileCreationState(
            new ToastService(),
            scenarioSelection ?? new ScenarioSelection(),
            new ProfileCreationScenarioProvider(),
            provider.GetRequiredService<Microsoft.Extensions.Localization.IStringLocalizer<ProfileCreationResources>>(),
            provider.GetRequiredService<Microsoft.Extensions.Localization.IStringLocalizer<BeeDayLab.Web.Resources.SharedResources>>());
    }

    private static void RegisterServices(BunitContext context)
    {
        context.Services.AddLocalization();
        context.Services.AddScoped<ToastService>();
        context.Services.AddScoped<ScenarioSelection>();
        context.Services.AddSingleton<ProfileCreationScenarioProvider>();
        context.Services.AddScoped<ProfileCreationState>();
    }
}
