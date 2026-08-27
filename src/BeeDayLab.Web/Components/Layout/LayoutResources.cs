namespace BeeDayLab.Web.Components.Layout;

/// <summary>
/// Marker type for resolving the shared layout/navigation resource catalog via
/// <c>IStringLocalizer&lt;LayoutResources&gt;</c>. Ported verbatim in shape from BeeDay.Web's
/// <c>Components/Layout/LayoutResources.cs</c> (Sprint 33.10, FE33-104) — only the namespace
/// changed, to mirror this Lab project's root namespace. Sprint 33.8/33.9's layout components
/// (NavigationItems, DesktopSidebar, MobileHeader, MobileSidebar, ReconnectModal, etc.) do not
/// consume this catalog themselves — see the Sprint 33.10 scope boundary on retrofitting already
/// shipped components; this marker type exists so the copied resx catalog is resolvable/testable,
/// for a future Sprint to opt into if it chooses.
/// </summary>
public sealed class LayoutResources;
