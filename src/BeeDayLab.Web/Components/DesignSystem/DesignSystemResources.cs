namespace BeeDayLab.Web.Components.DesignSystem;

/// <summary>
/// Marker type for resolving the Design System's shared default-copy resource catalog via
/// <c>IStringLocalizer&lt;DesignSystemResources&gt;</c>. Ported verbatim in shape from BeeDay.Web's
/// <c>Components/DesignSystem/DesignSystemResources.cs</c> (Sprint 33.10, FE33-104) — only the
/// namespace changed, to mirror this Lab project's root namespace. Sprint 33.8's Design System
/// components (BeeDayConfirmDialog, BeeDayLoading, EditorModalShell, BeeDayProgressBar, etc.) do
/// not consume this catalog themselves — see the Sprint 33.10 scope boundary on retrofitting
/// already shipped components; this marker type exists so the copied resx catalog is
/// resolvable/testable, for a future Sprint to opt into if it chooses.
/// </summary>
public sealed class DesignSystemResources;
