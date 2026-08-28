using BeeDayLab.Web.Components.Pages.Daily.Tasks.Models;
using BeeDayLab.Web.Scenarios;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;

namespace BeeDayLab.Web.Components.Pages.Daily.Tasks.Components;

/// <summary>
/// ADAPT (Sprint 33.13, FE33-092) of BeeDay.Web's
/// <c>Components/Features/Tasks/Components/TaskEditorModal.razor(.cs)</c>.
///
/// <para><b>Ledger correction:</b> the Ledger classifies this COPY, on the basis that it depends on
/// nothing beyond <c>EditorModalShell</c> (which the Lab already has from Sprint 33.8). The shell
/// dependency does check out, but the verification the brief asked for turned up one more: the
/// component renders a <c>BeeDaySelect TValue="TaskRepeat"</c> over
/// <c>Enum.GetValues&lt;TaskRepeat&gt;()</c> and localizes each member — <c>TaskRepeat</c> being a
/// <c>BeeDay.Domain.Enums</c> type. It is therefore ADAPT, not COPY: retyped to the Lab-local
/// <see cref="DailyTaskRepeat"/>. Everything else is verbatim.</para>
/// </summary>
public partial class TaskEditorModal
{
    [Inject] private IStringLocalizer<TaskResources> Localizer { get; set; } = default!;

    [Parameter, EditorRequired] public TaskEditorModel Model { get; set; } = new();
    [Parameter] public bool IsEditing { get; set; }
    [Parameter] public EventCallback<TaskEditorModel> OnSave { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback OnDelete { get; set; }
    [Parameter] public string? FallbackFocusSelector { get; set; }

    private bool showDeleteConfirmation;

    private Task Save() => OnSave.InvokeAsync(Model);

    private Task Cancel()
    {
        showDeleteConfirmation = false;
        return OnCancel.InvokeAsync();
    }

    private void RequestDelete() => showDeleteConfirmation = true;

    private void CloseDeleteConfirmation() => showDeleteConfirmation = false;

    private async Task ConfirmDelete()
    {
        showDeleteConfirmation = false;
        await OnDelete.InvokeAsync();
    }

    private Task HandleKeyDown(KeyboardEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Key != "Escape")
        {
            return Task.CompletedTask;
        }

        if (showDeleteConfirmation)
        {
            showDeleteConfirmation = false;
            return Task.CompletedTask;
        }

        return Cancel();
    }

    private string FormatRepeat(DailyTaskRepeat repeat) => repeat switch
    {
        DailyTaskRepeat.None => Localizer["RepeatNone"],
        DailyTaskRepeat.Daily => Localizer["RepeatDaily"],
        DailyTaskRepeat.Weekly => Localizer["RepeatWeekly"],
        DailyTaskRepeat.Monthly => Localizer["RepeatMonthly"],
        _ => repeat.ToString()
    };
}
