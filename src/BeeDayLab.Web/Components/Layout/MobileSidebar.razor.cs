using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BeeDayLab.Web.Components.Layout;

public partial class MobileSidebar
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    private ElementReference closeButtonElement;
    private bool wasOpen;

    private string AriaHiddenValue => IsOpen ? "false" : "true";

    private Task HandleNavigate() => OnClose.InvokeAsync();

    private Task HandleKeyDown(KeyboardEventArgs args) =>
        args.Key == "Escape" ? OnClose.InvokeAsync() : Task.CompletedTask;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsOpen && !wasOpen)
        {
            await closeButtonElement.FocusAsync();
        }

        wasOpen = IsOpen;
    }
}
