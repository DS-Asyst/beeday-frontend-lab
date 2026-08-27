namespace BeeDayLab.Web.Resources;

/// <summary>
/// Marker type for resolving the shared, cross-feature resource catalog via
/// <c>IStringLocalizer&lt;SharedResources&gt;</c>. Carries no members — its only purpose is to
/// anchor <c>SharedResources.{culture}.resx</c> resolution by namespace/type convention. Ported
/// verbatim in shape from BeeDay.Web's <c>Resources/SharedResources.cs</c> (Sprint 33.10,
/// FE33-104) — only the namespace changed, to mirror this Lab project's root namespace.
/// </summary>
public sealed class SharedResources;
