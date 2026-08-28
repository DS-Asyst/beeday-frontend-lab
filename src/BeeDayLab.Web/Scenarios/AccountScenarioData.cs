namespace BeeDayLab.Web.Scenarios;

/// <summary>
/// Lab-local stand-ins for <c>BeeDay.Domain.Enums.UserLanguage</c>/<c>UserTheme</c> — a Domain
/// dependency ADR-008 forbids in the Lab (the same reason <c>LabCultures.cs</c>, Sprint 33.10,
/// already dropped <c>FromUserLanguage</c>/<c>ToUserLanguage</c> rather than porting them). Only the
/// two values production's PreferencesSection.razor actually renders as options need representing
/// here; the culture-sync flow maps <see cref="AccountLanguage"/> straight to a literal culture code
/// ("en-US"/"pt-BR"), the same inlining <c>PublicLanguageSwitcher.razor</c> (Sprint 33.9) already
/// does for the same reason.
/// </summary>
public enum AccountLanguage
{
    English,
    Portuguese
}

/// <summary>Lab-local stand-in for <c>BeeDay.Domain.Enums.UserTheme</c> — see <see cref="AccountLanguage"/> remarks.</summary>
public enum AccountTheme
{
    System,
    Light,
    Dark
}

/// <summary>
/// Presentation-only scenario data for the Lab adaptation of Account.razor (Sprint 33.12, FE33-086)
/// — replaces the real <c>Store.GetCurrentUserAsync</c> load in <c>OnInitializedAsync</c> and backs
/// all three sections' independent save flows (Profile/Security/Preferences).
/// </summary>
/// <param name="Name">Synthetic display name pre-populating ProfileSection.</param>
/// <param name="Email">Synthetic email pre-populating ProfileSection.</param>
/// <param name="Nickname">Synthetic read-only nickname pre-populating ProfileSection.</param>
/// <param name="Language">Synthetic preferred language pre-populating PreferencesSection.</param>
/// <param name="Theme">Synthetic preferred theme pre-populating PreferencesSection.</param>
/// <param name="OperationSucceeds">
/// Whether a save action (any of the three independent sections) should appear to succeed for this
/// scenario — consulted by SaveProfileAsync/ChangePasswordAsync/SavePreferencesAsync after each
/// one's own fixed busy-delay, not just at initial load.
/// </param>
public sealed record AccountScenarioData(
    string Name,
    string Email,
    string Nickname,
    AccountLanguage Language,
    AccountTheme Theme,
    bool OperationSucceeds);
