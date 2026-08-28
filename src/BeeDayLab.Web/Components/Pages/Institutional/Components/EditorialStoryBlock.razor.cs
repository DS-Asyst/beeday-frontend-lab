using Microsoft.AspNetCore.Components;

namespace BeeDayLab.Web.Components.Pages.Institutional.Components;

public partial class EditorialStoryBlock
{
    [Parameter, EditorRequired] public string Headline { get; set; } = string.Empty;
    [Parameter] public string? Support { get; set; }

    /// <summary>The closing-statement look: centered, larger, no left alignment/support copy.</summary>
    [Parameter] public bool Centered { get; set; }
}
