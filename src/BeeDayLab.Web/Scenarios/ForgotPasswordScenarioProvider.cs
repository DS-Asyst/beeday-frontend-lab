namespace BeeDayLab.Web.Scenarios;

/// <summary>
/// Stateless/pure scenario provider (Sprint 33.12, FE33-080) for ForgotPassword.razor — Singleton
/// registration in Program.cs, same convention as <see cref="DemoCardListScenarioProvider"/>.
/// Mapping: <see cref="ScenarioState.Error"/> is the only state that produces a synthetic failure;
/// every other named state (including <see cref="ScenarioState.Loading"/>, whose in-flight UX is
/// already represented by the page's own fixed <c>Task.Delay</c> before this provider is consulted)
/// produces a synthetic success, matching production's enumeration-safe "always looks the same"
/// success response.
/// </summary>
public sealed class ForgotPasswordScenarioProvider : IScenarioProvider<ForgotPasswordScenarioData>
{
    /// <inheritdoc />
    public ForgotPasswordScenarioData GetScenario(ScenarioContext context) =>
        new(Succeeds: context.State != ScenarioState.Error);
}
