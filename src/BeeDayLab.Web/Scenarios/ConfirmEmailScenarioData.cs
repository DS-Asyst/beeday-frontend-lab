namespace BeeDayLab.Web.Scenarios;

/// <summary>
/// The 6 outcomes ConfirmEmail.razor's own <c>ConfirmationPageState</c> enum already distinguished
/// in production (via <c>Sender.Send(ConfirmEmailCommand)</c> + local exception-message
/// classification) — kept as the scenario data's shape per the Sprint 33.12 issue's explicit
/// guidance rather than collapsed down to a generic success/failure bool, since the page's whole
/// point is to preview these 6 distinct visual narratives.
/// </summary>
public enum ConfirmEmailOutcome
{
    Processing,
    Confirmed,
    AlreadyConfirmed,
    Expired,
    Replaced,
    Invalid
}

/// <summary>
/// Presentation-only scenario data for the Lab adaptation of ConfirmEmail.razor (Sprint 33.12,
/// FE33-083) — replaces the real <c>ISender.Send(ConfirmEmailCommand)</c> call and its exception
/// message classification.
/// </summary>
/// <param name="Outcome">Which of the 6 confirmation narratives to render.</param>
public sealed record ConfirmEmailScenarioData(ConfirmEmailOutcome Outcome);
