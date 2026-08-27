namespace BeeDayLab.Web.Scenarios;

/// <summary>
/// Presentation-only scenario data shared by the Lab adaptations of ResendConfirmation.razor
/// (FE33-081) and EmailConfirmationSent.razor (FE33-082, "idem FE33-081" per the Sprint 33.12
/// Ledger note) — both replace the real <c>ISender.Send(ResendEmailConfirmationCommand)</c> call
/// with the same success/failure shape.
/// </summary>
/// <param name="Succeeds">Whether the synthetic resend request should appear to succeed.</param>
public sealed record ResendConfirmationScenarioData(bool Succeeds);
