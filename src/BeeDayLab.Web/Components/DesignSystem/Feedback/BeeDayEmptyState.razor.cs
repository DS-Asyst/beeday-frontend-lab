using BeeDayLab.Web.Components.DesignSystem.Icons;
using Microsoft.AspNetCore.Components;

namespace BeeDayLab.Web.Components.DesignSystem.Feedback;

public partial class BeeDayEmptyState
{
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string Description { get; set; } = string.Empty;
    [Parameter] public BeeDayIconName? Icon { get; set; }
    [Parameter] public string? Class { get; set; }
}
