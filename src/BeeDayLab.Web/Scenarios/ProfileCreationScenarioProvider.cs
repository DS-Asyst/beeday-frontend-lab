namespace BeeDayLab.Web.Scenarios;

/// <summary>
/// Stateless/pure scenario provider (Sprint 33.12, FE33-079) for CreateProfile.razor — Singleton
/// registration in Program.cs. Same mapping as <see cref="ForgotPasswordScenarioProvider"/>: only
/// <see cref="ScenarioState.Error"/> produces a synthetic failure.
/// </summary>
public sealed class ProfileCreationScenarioProvider : IScenarioProvider<ProfileCreationScenarioData>
{
    /// <inheritdoc />
    public ProfileCreationScenarioData GetScenario(ScenarioContext context) =>
        new(Succeeds: context.State != ScenarioState.Error);
}
