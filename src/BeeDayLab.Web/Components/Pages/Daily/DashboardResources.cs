namespace BeeDayLab.Web.Components.Pages.Daily;

/// <summary>
/// Marker type for resolving the Dashboard resource catalog via
/// <c>IStringLocalizer&lt;DashboardResources&gt;</c>. Ported verbatim in shape from BeeDay.Web's
/// <c>Components/Features/Dashboard/DashboardResources.cs</c> (Sprint 33.13, FE33-088/090) — only
/// the namespace changed. Covers both Daily pages (/daily, /profile), their shared components
/// (ActivityFilterBar, DashboardColumn, ActivityCard, HabitCard, ProjectContextFilter) and
/// LabDashboardState's feedback messages — one catalog for the whole feature.
/// </summary>
public sealed class DashboardResources;
