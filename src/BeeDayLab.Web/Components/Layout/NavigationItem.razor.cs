using BeeDayLab.Web.Components.DesignSystem.Icons;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace BeeDayLab.Web.Components.Layout;

/// <summary>
/// Shared row primitive for BeeDay's authenticated navigation (desktop sidebar and mobile drawer):
/// either a route (<see cref="Href"/> set, renders a <see cref="NavLink"/>) or an action trigger
/// (<see cref="Href"/> null, renders a button). Carries no business logic; destinations and callbacks are supplied by
/// the caller.
/// </summary>
public partial class NavigationItem : IDisposable
{
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter, EditorRequired] public BeeDayIconName Icon { get; set; }
    [Parameter, EditorRequired] public string Label { get; set; } = string.Empty;

    /// <summary>Route mode when set; action mode when null.</summary>
    [Parameter] public string? Href { get; set; }
    [Parameter] public NavLinkMatch Match { get; set; } = NavLinkMatch.Prefix;

    /// <summary>
    /// Route mode only — an extra path that should also count as "current" for this item, for a page
    /// reachable at more than one route (e.g. Account's <c>/account</c> compatibility alias for
    /// <c>/settings</c>). <see cref="NavLink"/> itself only ever matches against <see cref="Href"/>,
    /// so this only affects <see cref="AriaCurrentValue"/> and the manually-applied active class.
    /// </summary>
    [Parameter] public string? AlternateHref { get; set; }

    /// <summary>Route mode only — invoked in addition to normal navigation (e.g. to close a mobile drawer).</summary>
    [Parameter] public EventCallback OnNavigate { get; set; }

    /// <summary>Action mode only.</summary>
    [Parameter] public EventCallback OnClick { get; set; }
    [Parameter] public string ButtonType { get; set; } = "button";
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public bool? AriaExpanded { get; set; }

    private string? AriaExpandedValue => AriaExpanded is null ? null : (AriaExpanded.Value ? "true" : "false");

    private string? AriaCurrentValue => Href is not null && IsCurrentRouteActive() ? "page" : null;

    private string? ActiveClassValue => IsCurrentRouteActive() ? "is-active" : null;

    protected override void OnInitialized() => NavigationManager.LocationChanged += HandleLocationChanged;

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs e) =>
        InvokeAsync(StateHasChanged);

    private Task HandleNavigate() => OnNavigate.InvokeAsync();

    private bool IsCurrentRouteActive()
    {
        if (Href is null)
        {
            return false;
        }

        var relativePath = NormalizePath(NavigationManager.ToBaseRelativePath(NavigationManager.Uri));

        return MatchesPath(relativePath, Href) ||
            (AlternateHref is not null && MatchesPath(relativePath, AlternateHref));
    }

    private bool MatchesPath(string relativePath, string href)
    {
        var hrefPath = NormalizePath(href);

        return Match == NavLinkMatch.All
            ? string.Equals(relativePath, hrefPath, StringComparison.OrdinalIgnoreCase)
            : relativePath.StartsWith(hrefPath, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string path)
    {
        var withoutQuery = path.Split('?', '#')[0];
        var trimmed = "/" + withoutQuery.Trim('/');
        return trimmed;
    }

    public void Dispose() => NavigationManager.LocationChanged -= HandleLocationChanged;
}
