using BeeDayLab.Web.Scenarios;

namespace BeeDayLab.Web.Components.Pages.Identity.Models;

/// <summary>
/// Lab adaptation (Sprint 33.12, FE33-086) of BeeDay.Web's
/// Components/Features/Account/Models/PreferencesFormModel.cs. Production's <c>Language</c>/
/// <c>Theme</c> properties are typed <c>BeeDay.Domain.Enums.UserLanguage</c>/<c>UserTheme</c> — a
/// Domain dependency ADR-008 forbids in the Lab. Reclassified COPY -&gt; ADAPT for this reason:
/// retyped to the Lab-local <see cref="AccountLanguage"/>/<see cref="AccountTheme"/> enums
/// (Scenarios/AccountScenarioData.cs), the same "Domain enum dropped, Lab-local literal/enum used
/// instead" treatment <c>LabCultures.cs</c> (Sprint 33.10) and <c>PublicLanguageSwitcher.razor</c>
/// (Sprint 33.9) already established.
/// </summary>
public sealed class PreferencesFormModel
{
    public AccountLanguage Language { get; set; } = AccountLanguage.English;
    public AccountTheme Theme { get; set; } = AccountTheme.System;
}
