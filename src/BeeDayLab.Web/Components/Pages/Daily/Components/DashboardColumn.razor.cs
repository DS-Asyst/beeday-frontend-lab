using BeeDayLab.Web.Components.DesignSystem.Icons;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace BeeDayLab.Web.Components.Pages.Daily.Components;

/// <summary>
/// COPY (Sprint 33.13, FE33-097) of BeeDay.Web's
/// <c>Components/Features/Dashboard/Components/DashboardColumn.razor(.cs)</c> — namespace and the
/// markup's <c>@using</c> line are the only changes.
///
/// <para><b>Ledger correction (same pattern Sprints 33.9/33.11 used for similar corrections):</b>
/// the EPIC 33 Ledger classifies this component ADAPT, with the reason given as "DashboardState
/// filtering". Verified against the actual file: it has no <c>DashboardState</c> dependency of any
/// kind, injects no service other than <c>IStringLocalizer&lt;DashboardResources&gt;</c>, and takes
/// only pre-computed counts, plain strings, an icon name, three <c>RenderFragment</c>s and two
/// <c>EventCallback</c>s. The filtering the Ledger refers to happens entirely in the caller
/// (<c>DailyHome.razor</c> passes <c>ActiveCount</c>/<c>CompletedCount</c>/<c>ShowClearFilterAction</c>
/// already resolved). It is therefore reclassified COPY and ported verbatim rather than given an
/// adaptation it does not need.</para>
///
/// <para>Production's own note, still true here: the strings that vary per grammatical state
/// (active/completed) or gender (Portuguese noun agreement differs by category — "hábitos ativos"
/// vs. "tarefas ativas") are supplied fully composed by the caller via DashboardResources, rather
/// than assembled here from fragments — composing "No completed {0}" generically would produce
/// grammatically wrong Portuguese for feminine categories like Tasks/To-Dos.</para>
/// </summary>
public partial class DashboardColumn
{
    [Inject] private IStringLocalizer<DashboardResources> Localizer { get; set; } = default!;

    private bool showCompleted;

    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string EmptyTitle { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string EmptyDescription { get; set; } = string.Empty;
    [Parameter] public BeeDayIconName EmptyIcon { get; set; } = BeeDayIconName.Information;
    [Parameter] public string? SingularLabel { get; set; }
    [Parameter] public int ActiveCount { get; set; }
    [Parameter] public int CompletedCount { get; set; }
    [Parameter] public bool ShowCompletedSection { get; set; } = true;
    [Parameter] public bool ShowCreateButton { get; set; } = true;
    [Parameter] public string? ActiveStateLabel { get; set; }
    [Parameter] public string? CompletedStateLabel { get; set; }
    [Parameter] public string? ShowActiveAriaLabel { get; set; }
    [Parameter] public string? ShowCompletedAriaLabel { get; set; }
    [Parameter] public string? CompletedEmptyTitle { get; set; }
    [Parameter] public string? CompletedEmptyDescription { get; set; }
    [Parameter] public EventCallback OnCreate { get; set; }
    [Parameter] public bool ShowClearFilterAction { get; set; }
    [Parameter] public EventCallback OnClearFilter { get; set; }
    [Parameter] public RenderFragment? HeaderContent { get; set; }
    [Parameter] public RenderFragment? ActiveContent { get; set; }
    [Parameter] public RenderFragment? CompletedContent { get; set; }

    private string NormalizedTitle => Title.ToLowerInvariant().Replace(" ", "-", StringComparison.Ordinal);
    private string HeadingId => $"dashboard-{NormalizedTitle}";
    private string ResolvedSingularLabel => string.IsNullOrWhiteSpace(SingularLabel) ? Title.TrimEnd('s') : SingularLabel;
    private BeeDayIconName CurrentViewIcon => showCompleted ? BeeDayIconName.Completed : BeeDayIconName.Repeat;
    private int CurrentCount => showCompleted && ShowCompletedSection ? CompletedCount : ActiveCount;
    private string CurrentCountLabel => $"{CurrentCount} {(showCompleted ? CompletedStateLabel : ActiveStateLabel)}";
    private string AriaPressed => showCompleted ? "true" : "false";
    private string ToggleViewLabel => showCompleted ? ShowActiveAriaLabel ?? string.Empty : ShowCompletedAriaLabel ?? string.Empty;
    private string AddItemAriaLabel => Localizer["DashboardAddItemAriaLabel", ResolvedSingularLabel];

    private void ToggleCompleted() => showCompleted = !showCompleted;
}
