using Microsoft.AspNetCore.Components;

namespace BeeDayLab.Web.Components.DesignSystem.Layout;

public partial class BeeDaySettingsSection
{
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter] public string? Eyebrow { get; set; }
    [Parameter] public string? Description { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string CssClass => string.Join(' ', new[] { "beeday-settings-section", Class }
        .Where(value => !string.IsNullOrWhiteSpace(value)));
}
