using Microsoft.AspNetCore.Components;

namespace BeeDayLab.Web.Components.Pages.Institutional.Components;

public partial class EditorialSectionNav
{
    [Parameter, EditorRequired] public EditorialSection Section { get; set; }

    /// <summary>The current page's own route, so its own link can be marked aria-current="page".</summary>
    [Parameter, EditorRequired] public string CurrentHref { get; set; } = string.Empty;

    private IReadOnlyList<EditorialSectionLink> Links => EditorialSectionRegistry.GetLinks(Section);
}
