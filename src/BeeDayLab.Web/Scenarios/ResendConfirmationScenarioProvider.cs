namespace BeeDayLab.Web.Scenarios;

/// <summary>
/// Stateless/pure scenario provider (Sprint 33.12, FE33-081/FE33-082) shared by ResendConfirmation
/// and EmailConfirmationSent — Singleton registration in Program.cs. Same mapping as
/// <see cref="ForgotPasswordScenarioProvider"/>: only <see cref="ScenarioState.Error"/> produces a
/// synthetic failure.
/// </summary>
public sealed class ResendConfirmationScenarioProvider : IScenarioProvider<ResendConfirmationScenarioData>
{
    /// <inheritdoc />
    public ResendConfirmationScenarioData GetScenario(ScenarioContext context) =>
        new(Succeeds: context.State != ScenarioState.Error);
}
