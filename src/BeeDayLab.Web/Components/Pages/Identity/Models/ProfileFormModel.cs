using System.ComponentModel.DataAnnotations;

namespace BeeDayLab.Web.Components.Pages.Identity.Models;

/// <summary>
/// Lab COPY, verbatim (Sprint 33.12, FE33-086), of BeeDay.Web's
/// Components/Features/Account/Models/ProfileFormModel.cs — plain DataAnnotations, zero BeeDay
/// dependency, so only the namespace changed.
/// </summary>
public sealed class ProfileFormModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must contain between 2 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    public string Nickname { get; set; } = string.Empty;

    /// <summary>
    /// Only required when Email actually changes — enforced in Account.razor before submit, not via
    /// a DataAnnotations [Required] here, since a Name-only save must not force this field.
    /// </summary>
    public string CurrentPassword { get; set; } = string.Empty;
}
