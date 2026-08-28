using BeeDayLab.Web.Components.Pages.Daily.Projects.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BeeDayLab.Web.Components.Pages.Daily.Projects.Components;

/// <summary>
/// COPY (Sprint 33.13, FE33-094) of BeeDay.Web's
/// <c>Components/Features/Projects/Components/ProjectEditorModal.razor(.cs)</c> — the Ledger's COPY
/// classification is confirmed by verification: the component depends only on
/// <c>EditorModalShell</c>/<c>BeeDayConfirmDialog</c> (both already in the Lab), its own
/// <c>ProjectEditorModel</c>, and <c>IStringLocalizer&lt;ProjectResources&gt;</c>. Its markup's
/// <c>@using BeeDay.Domain.Enums</c> directive resolves nothing the file actually uses and is
/// dropped; the code-behind carries no Domain/Application type at all.
///
/// <para>Production's unused <c>FormatEnum</c> helper (a private static regex that no markup or
/// method calls) is dropped rather than ported — it would fail the Lab's <c>--warnaserror</c> build
/// as dead private code, and it carries no presentation contract.</para>
/// </summary>
public partial class ProjectEditorModal
{
    [Parameter, EditorRequired] public ProjectEditorModel Model { get; set; } = new();
    [Parameter] public bool IsEditing { get; set; }
    [Parameter] public EventCallback<ProjectEditorModel> OnSave { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback OnDelete { get; set; }
    [Parameter] public string? FallbackFocusSelector { get; set; }
    [Parameter] public EventCallback OnOpenProject { get; set; }

    private bool showDeleteConfirmation;

    private Task Save() => OnSave.InvokeAsync(Model);

    private Task Cancel()
    {
        showDeleteConfirmation = false;
        return OnCancel.InvokeAsync();
    }

    private Task OpenProject() => OnOpenProject.InvokeAsync();

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
}
