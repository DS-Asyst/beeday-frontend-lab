using System.ComponentModel.DataAnnotations;

namespace BeeDayLab.Web.Components.Pages.Identity.Models;

/// <summary>
/// Lab COPY, verbatim (Sprint 33.12, FE33-079), of BeeDay.Web's
/// Components/Features/ProfileCreation/Models/ProfileCreationFormModel.cs — plain DataAnnotations,
/// zero BeeDay dependency, so only the namespace changed.
/// </summary>
public sealed class ProfileCreationFormModel
{
    [Required, StringLength(100, MinimumLength = 1)] public string Name { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(160)] public string Email { get; set; } = string.Empty;
    [Required, MinLength(8), StringLength(128)] public string Password { get; set; } = string.Empty;
    [Required, Compare(nameof(Password))] public string ConfirmPassword { get; set; } = string.Empty;
    [Required, MinLength(3), StringLength(24)] public string Nickname { get; set; } = string.Empty;
    [StringLength(500)] public string Avatar { get; set; } = string.Empty;
}
