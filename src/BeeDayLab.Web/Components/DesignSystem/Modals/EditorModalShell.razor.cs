using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BeeDayLab.Web.Components.DesignSystem.Modals;

public partial class EditorModalShell : IAsyncDisposable
{
    // Same module DialogFocusScope already loads for this dialog's focus-trap behavior - reused
    // here rather than a second JS file, since both are "move focus somewhere sensible within this
    // dialog" concerns. Sprint 32.5 (EXP32 Ledger): failed validation left focus on the submit
    // button instead of the first invalid field.
    // Lab adaptation (Sprint 33.8, FE33-106): dropped the hardcoded "?v=..." cache-busting query
    // string the BeeDay source carries on this import path — documented quality debt, not behavior
    // to preserve.
    private const string FocusModulePath = "./js/beeday-dialog-focus.js";

    private readonly string _dialogId = $"beeday-editor-dialog-{Guid.NewGuid():N}";

    [Inject] private IJSRuntime JS { get; set; } = default!;

    private IJSObjectReference? _focusModule;

    [Parameter, EditorRequired] public object Model { get; set; } = default!;
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string TitleId { get; set; } = string.Empty;
    [Parameter] public string? SubmitLabel { get; set; }
    [Parameter] public bool ShowDelete { get; set; }
    [Parameter] public bool IsBusy { get; set; }
    [Parameter] public string? ErrorMessage { get; set; }
    [Parameter] public string? FallbackFocusSelector { get; set; }
    [Parameter] public RenderFragment? HeroContent { get; set; }
    [Parameter] public RenderFragment? BodyContent { get; set; }
    [Parameter] public RenderFragment? SecondaryAction { get; set; }
    [Parameter] public EventCallback OnSubmit { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback OnDelete { get; set; }

    private string DialogId => _dialogId;

    private Task Submit(EditContext _) => IsBusy ? Task.CompletedTask : OnSubmit.InvokeAsync();

    private Task Cancel() => IsBusy ? Task.CompletedTask : OnCancel.InvokeAsync();

    private Task HandleKeyDown(KeyboardEventArgs args)
        => args.Key == "Escape" ? Cancel() : Task.CompletedTask;

    private async Task HandleInvalidSubmit()
    {
        // Yields so the renders StateHasChanged already queued for the now-invalid fields (raised
        // synchronously by EditContext.Validate() before this callback runs) reach the client
        // first - otherwise the .invalid marker this looks for would not exist in the DOM yet.
        await Task.Yield();

        try
        {
            _focusModule ??= await JS.InvokeAsync<IJSObjectReference>("import", FocusModulePath);
            await _focusModule.InvokeVoidAsync("focusFirstInvalid", DialogId);
        }
        catch (JSException)
        {
            // Focus management is progressive enhancement and must never break dialog content.
        }
        catch (JSDisconnectedException)
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_focusModule is not null)
        {
            try
            {
                await _focusModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
        }
    }
}
