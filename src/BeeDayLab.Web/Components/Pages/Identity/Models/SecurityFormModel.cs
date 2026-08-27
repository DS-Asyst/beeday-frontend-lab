using System.ComponentModel.DataAnnotations;

namespace BeeDayLab.Web.Components.Pages.Identity.Models;

/// <summary>
/// Lab COPY, verbatim (Sprint 33.12, FE33-086), of BeeDay.Web's
/// Components/Features/Account/Models/SecurityFormModel.cs — plain DataAnnotations, zero BeeDay
/// dependency, so only the namespace changed.
/// </summary>
public sealed class SecurityFormModel
{
    [Required(ErrorMessage = "Current password is required.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must contain between 8 and 128 characters.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one letter and one number.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm the new password.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;

    public void Clear()
    {
        CurrentPassword = string.Empty;
        NewPassword = string.Empty;
        ConfirmNewPassword = string.Empty;
    }
}
