namespace BeeDayLab.Web.Components.Pages.Institutional.Components;

/// <summary>
/// One contextual-navigation entry: a SharedResources link label key and its route. Ported
/// verbatim from BeeDay.Web's <c>Components/Features/Institutional/EditorialSectionLink.cs</c>
/// (Sprint 33.11, FE33-067) — only the namespace changed.
/// </summary>
public sealed record EditorialSectionLink(string LabelResourceKey, string Href);
