using BeeDayLab.Web.Components.Pages.Daily.Todos.Models;
using BeeDayLab.Web.Scenarios;
using Microsoft.AspNetCore.Components;

namespace BeeDayLab.Web.Components.Pages.Daily.Todos.Components;

/// <summary>
/// ADAPT (Sprint 33.13, FE33-093) of BeeDay.Web's
/// <c>Components/Features/Todos/Components/TodoEditorModal.razor(.cs)</c>.
///
/// <para><b>Ledger correction:</b> classified COPY in the Ledger; the verification the brief asked
/// for shows it takes <c>IReadOnlyList&lt;ProjectSummary&gt; Projects</c> — a
/// <c>BeeDay.Application.Features.Dashboard.Responses</c> type — to populate its project picker. It is
/// therefore ADAPT: <see cref="Projects"/> is retyped to <see cref="DailyProjectSummary"/> (the
/// markup reads only <c>Archived</c>, <c>Id</c> and <c>Name</c> from it). Everything else, including
/// the <c>EditorModalShell</c> composition, is verbatim.</para>
/// </summary>
public partial class TodoEditorModal
{
    [Parameter, EditorRequired] public TodoEditorModel Model { get; set; } = new();
    [Parameter] public IReadOnlyList<DailyProjectSummary> Projects { get; set; } = [];
    [Parameter] public bool IsEditing { get; set; }
    [Parameter] public EventCallback<TodoEditorModel> OnSave { get; set; }
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
}
