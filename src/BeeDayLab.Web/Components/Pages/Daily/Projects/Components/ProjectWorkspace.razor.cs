using BeeDayLab.Web.Scenarios;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;

namespace BeeDayLab.Web.Components.Pages.Daily.Projects.Components;

/// <summary>
/// ADAPT (Sprint 33.13, FE33-095) of BeeDay.Web's
/// <c>Components/Features/Projects/Components/ProjectWorkspace.razor(.cs/.css)</c> — a detail panel
/// rendered over <c>/daily</c> when a project is opened. Presentation-only apart from its types: the
/// five callbacks below are <c>EventCallback</c>s the caller (<c>DailyHome.razor</c>) wires to
/// <c>LabDashboardState</c>, so the panel itself neither knows nor cares that the mutations behind
/// them are local and inert.
///
/// <para>Adaptation is a straight retype of the three forbidden types it referenced —
/// <c>ProjectSummary</c>/<c>TodoSummary</c> (<c>BeeDay.Application.Features.Dashboard.Responses</c>)
/// and <c>ProjectStatus</c> (<c>BeeDay.Domain.Enums</c>) — to
/// <see cref="DailyProjectSummary"/>/<see cref="DailyTodoSummary"/>/<see cref="DailyProjectStatus"/>.
/// The <c>Project.Todos</c> the markup counts and lists are the nested todos
/// <c>LabDashboardState.OpenProject</c> re-nests, matching production's nested
/// <c>DashboardResponse</c> shape exactly.</para>
///
/// <para><c>ProgressPercentage</c> is rendered straight from the record — the panel's
/// "completed / total" line is a display count over the todos it was handed, never a recomputation of
/// the project's progress figure.</para>
/// </summary>
public partial class ProjectWorkspace
{
    [Inject] private IStringLocalizer<ProjectResources> Localizer { get; set; } = default!;

    [Parameter, EditorRequired] public DailyProjectSummary? Project { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnAddTodo { get; set; }
    [Parameter] public EventCallback<DailyTodoSummary> OnToggleTodo { get; set; }
    [Parameter] public EventCallback<DailyTodoSummary> OnEditTodo { get; set; }
    [Parameter] public EventCallback<DailyTodoSummary> OnDeleteTodo { get; set; }

    private bool showTodos = true;

    // Production seeds this from Guid.NewGuid() for a unique dialog/title id per instance. Kept as-is:
    // this is DOM-id uniqueness inside one rendered component, not scenario data, so the Lab's
    // scenario-determinism rule (which governs Scenarios/ only) does not apply.
    private readonly string id = Guid.NewGuid().ToString("N");

    private string DialogId => $"project-workspace-{id}";
    private string TitleId => $"project-workspace-title-{id}";

    private string StatusLabel => Project?.Status switch
    {
        DailyProjectStatus.InProgress => Localizer["StatusInProgress"],
        DailyProjectStatus.Completed => Localizer["StatusCompleted"],
        _ => Localizer["StatusPlanned"]
    };

    private string ToggleTodoAriaLabel(DailyTodoSummary todo) => todo.Completed
        ? Localizer["TodoMarkIncompleteAriaLabel", todo.Title]
        : Localizer["TodoCompleteAriaLabel", todo.Title];

    private void ToggleTodos() => showTodos = !showTodos;

    private Task HandleKeyDown(KeyboardEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        return args.Key == "Escape" ? OnClose.InvokeAsync() : Task.CompletedTask;
    }
}
