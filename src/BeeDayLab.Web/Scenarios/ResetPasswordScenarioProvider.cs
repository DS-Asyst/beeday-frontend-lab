namespace BeeDayLab.Web.Scenarios;

/// <summary>
/// Stateless/pure scenario provider (Sprint 33.12, FE33-084) for ResetPassword.razor — Singleton
/// registration in Program.cs. Same mapping as <see cref="ForgotPasswordScenarioProvider"/>: only
/// <see cref="ScenarioState.Error"/> produces a synthetic failure.
/// </summary>
public sealed class ResetPasswordScenarioProvider : IScenarioProvider<ResetPasswordScenarioData>
{
    /// <inheritdoc />
    public ResetPasswordScenarioData GetScenario(ScenarioContext context) =>
        new(Succeeds: context.State != ScenarioState.Error);
}
