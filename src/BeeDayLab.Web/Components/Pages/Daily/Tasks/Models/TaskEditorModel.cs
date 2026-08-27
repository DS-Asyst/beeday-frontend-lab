using System.ComponentModel.DataAnnotations;
using BeeDayLab.Web.Scenarios;

namespace BeeDayLab.Web.Components.Pages.Daily.Tasks.Models;

/// <summary>
/// ADAPT (Sprint 33.13, FE33-092) of BeeDay.Web's
/// <c>Components/Features/Tasks/Models/TaskEditorModel.cs</c> — DataAnnotations preserved verbatim,
/// <c>BeeDay.Domain.Enums.TaskRepeat</c>/<c>ActivityAttribute</c> retyped to the Lab-local
/// <see cref="DailyTaskRepeat"/>/<see cref="DailyActivityAttribute"/>.
/// </summary>
public sealed class TaskEditorModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string Description { get; set; } = string.Empty;

    public DailyTaskRepeat Repeat { get; set; } = DailyTaskRepeat.Daily;

    public DailyActivityAttribute? Attribute { get; set; }
}
