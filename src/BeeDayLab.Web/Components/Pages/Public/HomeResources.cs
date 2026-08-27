namespace BeeDayLab.Web.Components.Pages.Public;

/// <summary>
/// Marker type for resolving the public Home page's own resource catalog via
/// <c>IStringLocalizer&lt;HomeResources&gt;</c>. Ported verbatim in shape from BeeDay.Web's
/// <c>Components/Features/Home/HomeResources.cs</c> (Sprint 33.11, FE33-053) — only the namespace
/// changed, to mirror this Lab project's folder structure. Text shared with other public-chrome
/// components (e.g. the "Continue to beeday" CTA also shown by <c>PublicHeader</c>) lives in
/// <c>BeeDayLab.Web.Resources.SharedResources</c> instead — this catalog is only for copy that
/// belongs to the Home page itself.
/// </summary>
public sealed class HomeResources;
