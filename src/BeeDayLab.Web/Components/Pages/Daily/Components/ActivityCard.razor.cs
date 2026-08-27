using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;

namespace BeeDayLab.Web.Components.Pages.Daily.Components;

/// <summary>
/// COPY (Sprint 33.13, FE33-090 dependency) of BeeDay.Web's
/// <c>Components/Features/Dashboard/Components/ActivityCard.razor(.cs)</c> — namespace only.
/// Verified presentation-only: plain strings, two booleans and two <c>EventCallback</c>s, plus
/// <c>IStringLocalizer&lt;DashboardResources&gt;</c>. It renders Tasks, To-Dos and Projects alike,
/// discriminated by the plain <see cref="Variant"/> string, so it needs none of this Sprint's
/// Lab-local enums.
/// </summary>
public partial class ActivityCard
{
    [Inject] private IStringLocalizer<DashboardResources> Localizer { get; set; } = default!;

    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string Description { get; set; } = string.Empty;
    [Parameter] public string SearchTerm { get; set; } = string.Empty;
    [Parameter] public string Meta { get; set; } = string.Empty;
    [Parameter] public string Variant { get; set; } = "task";
    [Parameter] public bool Featured { get; set; }
    [Parameter] public bool Completed { get; set; }
    [Parameter] public EventCallback OnToggle { get; set; }
    [Parameter] public EventCallback OnEdit { get; set; }

    private string CardCssClass =>
        $"activity-card activity-card--{Variant} {(Completed ? "activity-card--completed" : string.Empty)}";

    private string EntityLabel => Variant switch
    {
        "todo" => Localizer["TodoSingular"],
        "project" => Localizer["ProjectSingular"],
        _ => Localizer["TaskSingular"]
    };

    private string ToggleAriaLabel => Completed
        ? Localizer["ActivityCardMarkIncompleteAriaLabel", Title]
        : Localizer["ActivityCardCompleteAriaLabel", Title];

    private string EditAriaLabel => Localizer["ActivityCardEditAriaLabel", EntityLabel, Title];

    private Task HandleBodyKeyDown(KeyboardEventArgs args) =>
        args.Key is "Enter" or " " ? OnEdit.InvokeAsync() : Task.CompletedTask;
}
