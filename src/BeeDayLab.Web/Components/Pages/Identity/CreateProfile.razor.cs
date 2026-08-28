using Microsoft.AspNetCore.Components;

namespace BeeDayLab.Web.Components.Pages.Identity;

/// <summary>
/// Lab adaptation (Sprint 33.12, FE33-079) of BeeDay.Web's
/// Components/Features/ProfileCreation/Pages/CreateProfile.razor.cs. Production injects
/// AuthenticationStateProvider + AuthenticatedUserInitializer (real auth infrastructure — EXCLUDE)
/// to decide whether the visitor already has an authenticated session with no profile yet. Replaced
/// with a plain <see cref="Authenticated"/> query-string toggle, same "Lab-local scenario state
/// instead of real auth" pattern Home.razor (Sprint 33.11) and PublicHeader.razor (Sprint 33.9)
/// already established — see "/profile/create?authenticated=true" for the authenticated-flow preview.
/// </summary>
public partial class CreateProfile
{
    /// <summary>
    /// Lab-local scenario toggle standing in for the real AuthenticationStateProvider check — set via
    /// the "authenticated" query string. <see langword="true"/> previews production's "authenticated,
    /// no profile yet" branch (starts at the Profile step with a synthetic name/email already filled
    /// in); <see langword="false"/> (default) previews anonymous registration (starts at the Account
    /// step), matching the Sprint 33.12 Ledger note ("Cenário decide qual dos 2 fluxos").
    /// </summary>
    [Parameter]
    [SupplyParameterFromQuery(Name = "authenticated")]
    public bool Authenticated { get; set; }

    protected override void OnInitialized() => State.Initialize(Authenticated);

    private void ContinueToProfile() => State.ContinueToProfile();
    private void Back() => State.Back();

    private async Task CompleteProfileAsync()
    {
        if (!await State.CompleteProfileAsync())
        {
            return;
        }

        if (Authenticated)
        {
            Navigation.NavigateTo("/onboarding/tutorial", forceLoad: true, replace: true);
            return;
        }

        Navigation.NavigateTo(
            $"/account/email-confirmation-sent?email={Uri.EscapeDataString(State.Model.Email.Trim())}",
            forceLoad: true,
            replace: true);
    }
}
