namespace BeeDayLab.Web.Components.Pages.Institutional;

/// <summary>
/// Marker type for resolving the 12 institutional/editorial pages' shared resource catalog via
/// <c>IStringLocalizer&lt;InstitutionalResources&gt;</c>. Ported verbatim in shape from
/// BeeDay.Web's <c>Components/Features/Institutional/InstitutionalResources.cs</c> (Sprint 33.11,
/// FE33-054..067) — only the namespace changed. Link label copy shared with AppFooter/
/// EditorialSectionNav (e.g. "Mission", "Contact us") lives in
/// <c>BeeDayLab.Web.Resources.SharedResources</c> instead, exactly as in production.
/// </summary>
public sealed class InstitutionalResources;
