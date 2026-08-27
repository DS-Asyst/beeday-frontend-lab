using Microsoft.AspNetCore.Components;

namespace BeeDayLab.Web.Components.DesignSystem.Layout;

public partial class BeeDayPageHeader
{
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter] public string? Eyebrow { get; set; }
    [Parameter] public string? Description { get; set; }
    [Parameter] public RenderFragment? Actions { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => string.Join(' ', new[] { "beeday-page-header", Class }
        .Where(value => !string.IsNullOrWhiteSpace(value)));
}
