using Microsoft.AspNetCore.Components;

namespace BeeDayLab.Web.Components.DesignSystem.Cards;

public partial class BeeDayCard
{
    [Parameter] public string Class { get; set; } = string.Empty;
    [Parameter] public bool Padded { get; set; }
    [Parameter] public bool Muted { get; set; }
    [Parameter] public bool Prominent { get; set; }
    [Parameter] public bool Interactive { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private bool HostsLiveRegionRole =>
        AdditionalAttributes?.TryGetValue("role", out var role) == true
        && role is string roleValue
        && (roleValue == "status" || roleValue == "alert");

    private string CssClass => string.Join(' ', new[]
    {
        "beeday-card",
        Padded ? "beeday-card--padded" : null,
        Muted ? "beeday-card--muted" : null,
        Prominent ? "beeday-card--prominent" : null,
        Interactive ? "beeday-card--interactive" : null,
        Class
    }.Where(value => !string.IsNullOrWhiteSpace(value)));
}
