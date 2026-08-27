using BeeDayLab.Web.Components.DesignSystem.Feedback;
using BeeDayLab.Web.Components.Pages.Identity.Models;
using BeeDayLab.Web.Resources;
using BeeDayLab.Web.Scenarios;
using Microsoft.Extensions.Localization;

namespace BeeDayLab.Web.Components.Pages.Identity.State;

/// <summary>
/// Lab adaptation (Sprint 33.12, FE33-079) of BeeDay.Web's
/// Components/Features/ProfileCreation/State/ProfileCreationState.cs. Client-side validation rules
/// (name required, email format, password length &gt;=8 + letter + digit, passwords match, nickname
/// format &gt;=3 chars alphanumeric/./_/-) are ported verbatim — presentation-layer form contracts
/// worth preserving per the Sprint 33.12 issue's item 3, not business logic.
///
/// The real <c>BeeDayWebService store</c> dependency (<c>CreateAccountAsync</c>/
/// <c>CompleteUserProfileAsync</c>) is replaced with <see cref="ScenarioSelection"/> +
/// <see cref="ProfileCreationScenarioProvider"/>. <c>DomainErrorLocalizer.Translate(ex, sharedLocalizer)</c>
/// is dropped entirely (EXCLUDE, real Domain/Application exception classification); a failure instead
/// surfaces <c>SharedResources</c>'s already-real, already-localized "DomainErrorGeneric" string.
/// <c>ToastService</c> (Sprint 33.8) is reused directly for the success toast, unchanged.
/// </summary>
public sealed class ProfileCreationState(
    ToastService toastService,
    ScenarioSelection scenarioSelection,
    ProfileCreationScenarioProvider scenarioProvider,
    IStringLocalizer<ProfileCreationResources> localizer,
    IStringLocalizer<SharedResources> sharedLocalizer) : IDisposable
{
    // Scoped to this circuit's lifetime (ProfileCreationState itself is registered Scoped in
    // Program.cs) — cancels the in-flight synthetic delay this instance started once the circuit
    // ends, matching production's own cancellation-on-dispose pattern.
    private readonly CancellationTokenSource cancellation = new();

    public ProfileCreationFormModel Model { get; } = new();
    public ProfileCreationStep Step { get; private set; } = ProfileCreationStep.Account;
    public bool IsBusy { get; private set; }
    public string? ValidationError { get; private set; }
    public bool HasAuthenticatedSession { get; private set; }

    public void Dispose()
    {
        cancellation.Cancel();
        cancellation.Dispose();
    }

    public string NormalizedName => Model.Name.Trim();
    public string NormalizedNickname => Model.Nickname.Trim().TrimStart('@');
    public string CurrentStepClass => Step.ToString().ToLowerInvariant();

    public bool IsPasswordValid =>
        Model.Password.Length >= 8 &&
        Model.Password.Any(char.IsLetter) &&
        Model.Password.Any(char.IsDigit);

    public bool ShouldShowConfirmPassword => IsPasswordValid;

    public bool CanContinueAccount =>
        !string.IsNullOrWhiteSpace(NormalizedName) &&
        !string.IsNullOrWhiteSpace(Model.Email) &&
        Model.Email.Contains('@', StringComparison.Ordinal) &&
        IsPasswordValid &&
        !string.IsNullOrEmpty(Model.ConfirmPassword) &&
        string.Equals(Model.Password, Model.ConfirmPassword, StringComparison.Ordinal);

    public bool CanCompleteProfile =>
        NormalizedNickname.Length >= 3 &&
        NormalizedNickname.All(character =>
            char.IsLetterOrDigit(character) || character is '.' or '_' or '-');

    /// <summary>
    /// Lab replacement for production's real <c>AuthenticationStateProvider.GetAuthenticationStateAsync()</c>
    /// + <c>AuthenticatedUserInitializer.EnsureInitializedAsync()</c> + <c>store.GetCurrentUserAsync()</c>
    /// branch (which decided whether the visitor already had a profile and where to redirect). The
    /// Lab has no auth/account concept, so the page's own
    /// <c>[SupplyParameterFromQuery(Name="authenticated")]</c> toggle (see CreateProfile.razor.cs)
    /// directly decides which of the 2 flows to preview — matching the Sprint 33.12 Ledger note
    /// ("Cenário decide qual dos 2 fluxos"): <see langword="true"/> pre-populates a synthetic
    /// authenticated name/email and starts at the Profile step (production's real "authenticated, no
    /// profile yet" branch); <see langword="false"/> (default) starts at the Account step for
    /// anonymous registration.
    /// </summary>
    public void Initialize(bool hasAuthenticatedSession)
    {
        HasAuthenticatedSession = hasAuthenticatedSession;
        ValidationError = null;

        if (!hasAuthenticatedSession)
        {
            Step = ProfileCreationStep.Account;
            Model.Name = string.Empty;
            Model.Email = string.Empty;
            Model.Password = string.Empty;
            Model.ConfirmPassword = string.Empty;
            Model.Nickname = string.Empty;
            return;
        }

        Model.Name = "Jordan Silva";
        Model.Email = "jordan.silva@example.com";
        Step = ProfileCreationStep.Profile;
    }

    public bool ContinueToProfile()
    {
        ValidationError = null;

        if (string.IsNullOrWhiteSpace(NormalizedName))
        {
            ValidationError = localizer["FullNameRequired"];
            return false;
        }

        if (string.IsNullOrWhiteSpace(Model.Email) || !Model.Email.Contains('@', StringComparison.Ordinal))
        {
            ValidationError = localizer["ValidEmailRequired"];
            return false;
        }

        if (!IsPasswordValid)
        {
            ValidationError = localizer["PasswordRequirementsError"];
            return false;
        }

        if (!string.Equals(Model.Password, Model.ConfirmPassword, StringComparison.Ordinal))
        {
            ValidationError = localizer["PasswordsDoNotMatch"];
            return false;
        }

        Step = ProfileCreationStep.Profile;
        return true;
    }

    public void Back()
    {
        ValidationError = null;
        Step = ProfileCreationStep.Account;
    }

    public async Task<bool> CompleteProfileAsync()
    {
        if (IsBusy)
        {
            return false;
        }

        ValidationError = null;

        if (!CanCompleteProfile)
        {
            ValidationError = localizer["NicknameRequirementsError"];
            return false;
        }

        Model.Nickname = NormalizedNickname;
        IsBusy = true;

        try
        {
            // Replaces the real store.CreateAccountAsync/store.CompleteUserProfileAsync calls: a
            // fixed delay preserves the loading-state UX without a real network call, then the
            // scenario provider decides the outcome.
            await Task.Delay(400, cancellation.Token);
            var scenario = scenarioProvider.GetScenario(scenarioSelection.Context);

            if (!scenario.Succeeds)
            {
                ValidationError = sharedLocalizer["DomainErrorGeneric"];
                toastService.ShowError(ValidationError);
                return false;
            }

            toastService.ShowSuccess(localizer["WelcomeToast"]);
            return true;
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public enum ProfileCreationStep
{
    Account,
    Profile
}
