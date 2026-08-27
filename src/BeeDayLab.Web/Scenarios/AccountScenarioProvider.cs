namespace BeeDayLab.Web.Scenarios;

/// <summary>
/// Stateless/pure scenario provider (Sprint 33.12, FE33-086) for Account.razor — Singleton
/// registration in Program.cs. Every named state returns the same realistic synthetic profile (the
/// page always needs something to render across its three sections); only
/// <see cref="ScenarioState.Error"/> flips <see cref="AccountScenarioData.OperationSucceeds"/> to
/// <see langword="false"/>, which each of the three independent save flows
/// (SaveProfileAsync/ChangePasswordAsync/SavePreferencesAsync) checks after its own busy-delay to
/// decide whether to show a success or a synthetic-failure toast. This provider deliberately does
/// not fail the page's own initial load for <see cref="ScenarioState.Error"/> — that would leave all
/// three sections empty and unable to demonstrate their own independent failing-save narrative,
/// which is the more useful preview for this page.
/// </summary>
public sealed class AccountScenarioProvider : IScenarioProvider<AccountScenarioData>
{
    private const string SyntheticName = "Jordan Silva";
    private const string SyntheticEmail = "jordan.silva@example.com";
    private const string SyntheticNickname = "jordan.silva";

    /// <inheritdoc />
    public AccountScenarioData GetScenario(ScenarioContext context) => new(
        SyntheticName,
        SyntheticEmail,
        SyntheticNickname,
        AccountLanguage.English,
        AccountTheme.System,
        OperationSucceeds: context.State != ScenarioState.Error);
}
