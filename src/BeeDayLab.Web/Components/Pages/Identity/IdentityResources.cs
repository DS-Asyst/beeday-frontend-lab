namespace BeeDayLab.Web.Components.Pages.Identity;

/// <summary>
/// Marker type for resolving the account-recovery/email-confirmation pages' shared resource catalog
/// via <c>IStringLocalizer&lt;IdentityResources&gt;</c>. Ported verbatim in shape from BeeDay.Web's
/// <c>Components/Features/Identity/IdentityResources.cs</c> (Sprint 33.12, FE33-080..084) — shared
/// across ForgotPassword, ResendConfirmation, EmailConfirmationSent, ConfirmEmail and ResetPassword,
/// same grouping rationale production uses.
/// </summary>
public sealed class IdentityResources;
