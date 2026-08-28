namespace BeeDayLab.Web.Scenarios;

/// <summary>
/// Presentation-only scenario data for the Lab adaptation of ForgotPassword.razor (Sprint 33.12,
/// FE33-080) — replaces the real <c>ISender.Send(RequestPasswordResetCommand)</c> call. Production
/// always shows the same "submitted" success message regardless of whether the email exists (an
/// intentional enumeration-safe response), so the only outcome this Sprint needs to preview is
/// whether the synthetic request succeeds or fails.
/// </summary>
/// <param name="Succeeds">Whether the synthetic password-reset request should appear to succeed.</param>
public sealed record ForgotPasswordScenarioData(bool Succeeds);
