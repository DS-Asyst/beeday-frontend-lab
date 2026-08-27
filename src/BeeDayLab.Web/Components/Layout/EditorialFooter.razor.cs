using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BeeDayLab.Web.Components.Layout;

public sealed partial class EditorialFooter : IAsyncDisposable
{
    // Sprint 33.9 (FE33-049): dropped the "?v=20260818-1" cache-busting query suffix the BeeDay
    // source carries — same documented quality debt Sprint 33.8 chose not to preserve for
    // beeday-dialog-focus.js/beeday-sortable.js (query-string-free "./js/beeday-*.js" import paths).
    private const string ModulePath = "./js/beeday-editorial-footer.js";

    [Inject] private IJSRuntime JS { get; set; } = default!;

    private IJSObjectReference? _module;

    private async Task ScrollToTopAsync()
    {
        try
        {
            _module ??= await JS.InvokeAsync<IJSObjectReference>("import", ModulePath);
            await _module.InvokeVoidAsync("scrollToTop");
        }
        catch (InvalidOperationException)
        {
            // Static prerendering has no JavaScript runtime; nothing to scroll yet.
        }
        catch (JSException)
        {
            // Scrolling is progressive enhancement and must never break navigation.
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
            catch (JSException)
            {
            }
        }
    }
}
