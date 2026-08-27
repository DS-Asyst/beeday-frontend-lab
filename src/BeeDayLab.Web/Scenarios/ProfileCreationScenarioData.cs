namespace BeeDayLab.Web.Scenarios;

/// <summary>
/// Presentation-only scenario data for the Lab adaptation of CreateProfile.razor (Sprint 33.12,
/// FE33-079) — replaces the real <c>store.CreateAccountAsync</c>/<c>store.CompleteUserProfileAsync</c>
/// calls made from <c>ProfileCreationState.CompleteProfileAsync</c>.
/// </summary>
/// <param name="Succeeds">Whether the synthetic account-creation/profile-completion call should appear to succeed.</param>
public sealed record ProfileCreationScenarioData(bool Succeeds);
