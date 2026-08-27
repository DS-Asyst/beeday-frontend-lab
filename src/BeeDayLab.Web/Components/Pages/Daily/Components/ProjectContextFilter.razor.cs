using BeeDayLab.Web.Scenarios;
using Microsoft.AspNetCore.Components;

namespace BeeDayLab.Web.Components.Pages.Daily.Components;

/// <summary>
/// ADAPT (Sprint 33.13, FE33-090 dependency) of BeeDay.Web's
/// <c>Components/Features/Dashboard/Components/ProjectContextFilter.razor(.cs)</c>. Pure listbox
/// presentation — the only change is retyping <see cref="Projects"/> from
/// <c>BeeDay.Application.Features.Dashboard.Responses.ProjectSummary</c> to the Lab-local
/// <see cref="DailyProjectSummary"/>. It reads only <c>Id</c> and <c>Name</c>.
/// </summary>
public partial class ProjectContextFilter
{
    private bool isOpen;

    [Parameter] public IReadOnlyList<DailyProjectSummary> Projects { get; set; } = [];
    [Parameter] public Guid? SelectedProjectId { get; set; }
    [Parameter] public EventCallback<Guid?> SelectedProjectIdChanged { get; set; }

    private void ToggleMenu() => isOpen = !isOpen;

    private Task SelectAsync(Guid? projectId)
    {
        isOpen = false;
        return SelectedProjectIdChanged.InvokeAsync(projectId);
    }
}
