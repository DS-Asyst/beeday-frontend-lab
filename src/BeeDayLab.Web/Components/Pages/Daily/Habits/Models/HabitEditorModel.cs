using System.ComponentModel.DataAnnotations;
using BeeDayLab.Web.Scenarios;

namespace BeeDayLab.Web.Components.Pages.Daily.Habits.Models;

/// <summary>
/// ADAPT (Sprint 33.13, FE33-091) of BeeDay.Web's
/// <c>Components/Features/Habits/Models/HabitEditorModel.cs</c>. Every DataAnnotations rule is
/// preserved verbatim — client-side form contracts are presentation, not business logic, the same
/// judgement Sprint 33.12 applied to <c>ProfileCreationState</c>. The only change is retyping the
/// four <c>BeeDay.Domain.Enums</c> members (<c>HabitDirection</c>/<c>HabitDifficulty</c>/
/// <c>HabitResetCounter</c>/<c>ActivityAttribute</c>) to their Lab-local equivalents in
/// <c>Scenarios/DailyDashboardScenarioData.cs</c>.
/// </summary>
public sealed class HabitEditorModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string Description { get; set; } = string.Empty;

    public DailyHabitDirection Direction { get; set; } = DailyHabitDirection.Both;
    public DailyHabitDifficulty Difficulty { get; set; } = DailyHabitDifficulty.Easy;
    public DailyHabitResetCounter ResetCounter { get; set; } = DailyHabitResetCounter.Daily;

    public DailyActivityAttribute? Attribute { get; set; }

    /// <summary>
    /// The already-resolved balance the editor's own colour band is read from
    /// (<c>HabitVisualState.GetEditorClass</c>). Seeded from scenario data when an existing habit is
    /// opened; never recalculated.
    /// </summary>
    public int VisualBalance { get; set; }
}
