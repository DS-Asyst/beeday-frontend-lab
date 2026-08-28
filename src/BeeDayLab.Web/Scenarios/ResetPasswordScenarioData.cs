namespace BeeDayLab.Web.Scenarios;

/// <summary>
/// Presentation-only scenario data for the Lab adaptation of ResetPassword.razor (Sprint 33.12,
/// FE33-084) — replaces the real <c>ISender.Send(ResetPasswordCommand)</c> call.
/// </summary>
/// <param name="Succeeds">Whether the synthetic password-reset submission should appear to succeed.</param>
public sealed record ResetPasswordScenarioData(bool Succeeds);
