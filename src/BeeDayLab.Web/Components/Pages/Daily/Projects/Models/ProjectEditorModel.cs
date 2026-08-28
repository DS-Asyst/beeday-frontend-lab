using System.ComponentModel.DataAnnotations;
using BeeDayLab.Web.Scenarios;

namespace BeeDayLab.Web.Components.Pages.Daily.Projects.Models;

/// <summary>
/// ADAPT (Sprint 33.13, FE33-094) of BeeDay.Web's
/// <c>Components/Features/Projects/Models/ProjectEditorModel.cs</c> — DataAnnotations preserved
/// verbatim, <c>BeeDay.Domain.Enums.ActivityAttribute</c> retyped to
/// <see cref="DailyActivityAttribute"/>.
/// </summary>
public sealed class ProjectEditorModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string Description { get; set; } = string.Empty;

    // Projects no longer expose manual color selection — every project uses the fixed
    // --beeday-color-project Design System accent. Production keeps it as a plain string only
    // because the Domain's ProjectColor value object still requires one; the Lab has no Domain at
    // all, and the field is kept purely so the editor model mirrors production's shape.
    [Required]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Use a valid hexadecimal color.")]
    public string Color { get; set; } = ProjectAccentColor;

    public const string ProjectAccentColor = "#8056C7";

    public DateTime? ExpectedDate { get; set; }
    public bool Archived { get; set; }

    public DailyActivityAttribute? Attribute { get; set; }
}
