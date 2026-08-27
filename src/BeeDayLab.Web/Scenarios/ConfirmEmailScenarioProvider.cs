namespace BeeDayLab.Web.Scenarios;

/// <summary>
/// Stateless/pure scenario provider (Sprint 33.12, FE33-083) for ConfirmEmail.razor — Singleton
/// registration in Program.cs.
///
/// <para><b>Mapping choice</b> (the 8 generic <see cref="ScenarioState"/> values onto the page's own
/// 6-value <see cref="ConfirmEmailOutcome"/>, per the Sprint 33.12 issue's own suggested mapping):</para>
/// <list type="bullet">
/// <item><see cref="ScenarioState.Loading"/> → <see cref="ConfirmEmailOutcome.Processing"/> (the
/// "please wait" state, the closest narrative match).</item>
/// <item><see cref="ScenarioState.Populated"/> → <see cref="ConfirmEmailOutcome.Confirmed"/> (the
/// common, successful case).</item>
/// <item><see cref="ScenarioState.Selected"/> → <see cref="ConfirmEmailOutcome.AlreadyConfirmed"/>
/// ("selected/completed" reads naturally as "this was already done before").</item>
/// <item><see cref="ScenarioState.Disabled"/> → <see cref="ConfirmEmailOutcome.Expired"/>
/// (read-only/non-interactive maps to a link that can no longer be acted on).</item>
/// <item><see cref="ScenarioState.NoResults"/> → <see cref="ConfirmEmailOutcome.Replaced"/> (the
/// original link is no longer the relevant one — a newer one superseded it).</item>
/// <item><see cref="ScenarioState.Error"/> → <see cref="ConfirmEmailOutcome.Invalid"/> (the
/// generic failure narrative).</item>
/// <item><see cref="ScenarioState.Empty"/> and <see cref="ScenarioState.LargeContent"/> have no
/// natural narrative fit for a single-token confirmation flow — both fall back to
/// <see cref="ConfirmEmailOutcome.Invalid"/>, the same "no meaningful token" reading the page's own
/// missing-token branch already uses.</item>
/// </list>
/// </summary>
public sealed class ConfirmEmailScenarioProvider : IScenarioProvider<ConfirmEmailScenarioData>
{
    /// <inheritdoc />
    public ConfirmEmailScenarioData GetScenario(ScenarioContext context) => context.State switch
    {
        ScenarioState.Loading => new ConfirmEmailScenarioData(ConfirmEmailOutcome.Processing),
        ScenarioState.Populated => new ConfirmEmailScenarioData(ConfirmEmailOutcome.Confirmed),
        ScenarioState.Selected => new ConfirmEmailScenarioData(ConfirmEmailOutcome.AlreadyConfirmed),
        ScenarioState.Disabled => new ConfirmEmailScenarioData(ConfirmEmailOutcome.Expired),
        ScenarioState.NoResults => new ConfirmEmailScenarioData(ConfirmEmailOutcome.Replaced),
        ScenarioState.Error => new ConfirmEmailScenarioData(ConfirmEmailOutcome.Invalid),
        _ => new ConfirmEmailScenarioData(ConfirmEmailOutcome.Invalid)
    };
}
