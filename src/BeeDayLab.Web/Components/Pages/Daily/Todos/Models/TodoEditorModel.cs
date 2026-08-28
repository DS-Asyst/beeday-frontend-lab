using System.ComponentModel.DataAnnotations;
using BeeDayLab.Web.Scenarios;

namespace BeeDayLab.Web.Components.Pages.Daily.Todos.Models;

/// <summary>
/// ADAPT (Sprint 33.13, FE33-093) of BeeDay.Web's
/// <c>Components/Features/Todos/Models/TodoEditorModel.cs</c> — DataAnnotations preserved verbatim,
/// <c>BeeDay.Domain.Enums.ActivityAttribute</c> retyped to <see cref="DailyActivityAttribute"/>.
/// </summary>
public sealed class TodoEditorModel
{
    [Required(ErrorMessage = "Project is required.")]
    public Guid? ProjectId { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
    public string Description { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    public DailyActivityAttribute? Attribute { get; set; }
}
