namespace BeeDayLab.Web.Scenarios;

/// <summary>
/// Optional viewport/presentation preset a scenario lookup may care about (Sprint 33.10, Issue
/// #371 — "locale selection and viewport/presentation presets where useful"). Nullable on
/// <see cref="ScenarioContext"/>: most scenarios are viewport-agnostic, so a provider is free to
/// ignore <see cref="ScenarioContext.Viewport"/> entirely unless its data genuinely differs by
/// breakpoint (e.g. how many items fit "above the fold").
/// </summary>
public enum ViewportPreset
{
    Desktop,
    Tablet,
    Mobile
}
