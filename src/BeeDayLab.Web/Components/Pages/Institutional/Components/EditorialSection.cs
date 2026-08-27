namespace BeeDayLab.Web.Components.Pages.Institutional.Components;

/// <summary>
/// The footer-derived families that group the public editorial pages. Ported verbatim from
/// BeeDay.Web's <c>Components/Features/Institutional/EditorialSection.cs</c> (Sprint 33.11,
/// FE33-067) — only the namespace changed. Membership mirrors AppFooter.razor's own groups
/// exactly — this is not a separate taxonomy, it is the deterministic key
/// <see cref="EditorialSectionRegistry"/> uses to resolve each family's contextual navigation.
/// Social links are intentionally not represented: they are external and never become editorial
/// pages.
/// </summary>
public enum EditorialSection
{
    AboutUs,
    Products,
    Apps,
    Help,
    Legal
}
