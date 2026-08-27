namespace BeeDayLab.Web.Scenarios;

/// <summary>
/// The ambient selection a scenario provider is asked to render for (Sprint 33.10, Issue #371):
/// which named <see cref="ScenarioState"/>, which culture, and optionally which
/// <see cref="ViewportPreset"/>. Plain immutable data, no behavior — equality is structural
/// (record), which is what makes <see cref="IScenarioProvider{TData}.GetScenario"/>'s determinism
/// contract testable: the same <see cref="ScenarioContext"/> value must always produce an equal
/// result.
/// </summary>
/// <param name="State">Which named presentation state to render.</param>
/// <param name="Culture">
/// The culture code to render for (e.g. <c>"en-US"</c>, <c>"pt-BR"</c>). A provider whose synthetic
/// data has no locale-varying content is free to ignore this.
/// </param>
/// <param name="Viewport">
/// Optional viewport/presentation preset. <see langword="null"/> when the caller/provider doesn't
/// need one — most scenarios don't.
/// </param>
public sealed record ScenarioContext(ScenarioState State, string Culture, ViewportPreset? Viewport = null);
