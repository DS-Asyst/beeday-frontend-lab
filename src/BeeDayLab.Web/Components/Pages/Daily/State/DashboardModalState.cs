using BeeDayLab.Web.Components.Pages.Daily.Habits.Models;
using BeeDayLab.Web.Components.Pages.Daily.Projects.Models;
using BeeDayLab.Web.Components.Pages.Daily.Tasks.Models;
using BeeDayLab.Web.Components.Pages.Daily.Todos.Models;
using BeeDayLab.Web.Scenarios;

namespace BeeDayLab.Web.Components.Pages.Daily.State;

/// <summary>
/// ADAPT (Sprint 33.13, FE33-090) of BeeDay.Web's
/// <c>Components/Features/Dashboard/State/DashboardModalState.cs</c>. Verified on reading: this type
/// is 100% presentation-only bookkeeping — which editor dialog is open, which item id it is editing,
/// and the four form models pre-populated from the item being edited. It injects nothing, calls no
/// service and performs no persistence, so every method below is ported verbatim in behavior.
///
/// <para>The only changes are type substitutions, all resolved through this Sprint's single Lab-local
/// translation layer: <c>ActivityType</c> → <see cref="DailyActivityType"/>, and the four
/// <c>BeeDay.Application.Features.Dashboard.Responses</c> summaries → their <c>Daily*</c>
/// equivalents.</para>
/// </summary>
public sealed class DashboardModalState
{
    public Guid? EditingId { get; private set; }
    public DailyActivityType? ActiveEditor { get; private set; }
    public HabitEditorModel HabitForm { get; private set; } = new();
    public TaskEditorModel TaskForm { get; private set; } = new();
    public TodoEditorModel TodoForm { get; private set; } = new();
    public ProjectEditorModel ProjectForm { get; private set; } = new();

    public bool IsEditing => EditingId is not null;
    public bool IsHabitEditorOpen => ActiveEditor == DailyActivityType.Habit;
    public bool IsTaskEditorOpen => ActiveEditor == DailyActivityType.Task;
    public bool IsTodoEditorOpen => ActiveEditor == DailyActivityType.Todo;
    public bool IsProjectEditorOpen => ActiveEditor == DailyActivityType.Project;

    public void OpenCreate(DailyActivityType type)
    {
        EditingId = null;
        ActiveEditor = type;

        switch (type)
        {
            case DailyActivityType.Habit:
                HabitForm = new HabitEditorModel();
                break;
            case DailyActivityType.Task:
                TaskForm = new TaskEditorModel();
                break;
            case DailyActivityType.Todo:
                TodoForm = new TodoEditorModel();
                break;
            case DailyActivityType.Project:
                ProjectForm = new ProjectEditorModel();
                break;
        }
    }

    public void OpenTodoForProject(Guid projectId)
    {
        EditingId = null;
        TodoForm = new TodoEditorModel { ProjectId = projectId };
        ActiveEditor = DailyActivityType.Todo;
    }

    public void OpenHabit(DailyHabitSummary item)
    {
        ArgumentNullException.ThrowIfNull(item);

        EditingId = item.Id;
        HabitForm = new HabitEditorModel
        {
            Title = item.Title,
            Description = item.Description,
            Direction = item.Direction,
            Difficulty = item.Difficulty,
            ResetCounter = item.ResetCounter,
            Attribute = item.Attribute,
            VisualBalance = item.PositiveCount - item.NegativeCount
        };
        ActiveEditor = DailyActivityType.Habit;
    }

    public void OpenTask(DailyTaskSummary item)
    {
        ArgumentNullException.ThrowIfNull(item);

        EditingId = item.Id;
        TaskForm = new TaskEditorModel
        {
            Title = item.Title,
            Description = item.Description,
            Repeat = item.Repeat,
            Attribute = item.Attribute
        };
        ActiveEditor = DailyActivityType.Task;
    }

    public void OpenTodo(DailyTodoSummary item)
    {
        ArgumentNullException.ThrowIfNull(item);

        EditingId = item.Id;
        TodoForm = new TodoEditorModel
        {
            Title = item.Title,
            Description = item.Description,
            DueDate = item.DueDate?.ToDateTime(TimeOnly.MinValue),
            ProjectId = item.ProjectId,
            Attribute = item.Attribute
        };
        ActiveEditor = DailyActivityType.Todo;
    }

    public void OpenProject(DailyProjectSummary item)
    {
        ArgumentNullException.ThrowIfNull(item);

        EditingId = item.Id;
        ProjectForm = new ProjectEditorModel
        {
            Title = item.Name,
            Description = item.Description,
            ExpectedDate = item.ExpectedDate?.ToDateTime(TimeOnly.MinValue),
            Archived = item.Archived,
            Attribute = item.Attribute
        };
        ActiveEditor = DailyActivityType.Project;
    }

    public void CloseEditor()
    {
        ActiveEditor = null;
        EditingId = null;
    }
}
