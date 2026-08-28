namespace BeeDayLab.Web.Scenarios;

/// <summary>One synthetic card's worth of presentation data for <see cref="DemoCardListScenarioProvider"/>.</summary>
public sealed record DemoCardItem(string Title, string Subtitle);

/// <summary>
/// Presentation-only scenario data for the illustrative <see cref="DemoCardListScenarioProvider"/>
/// example (Sprint 33.10) — the minimal shape a real feature's own scenario data model (e.g. a
/// future <c>WalletScenarioData</c>) would follow: plain records, no Domain/Application types.
/// </summary>
/// <param name="Items">The cards to render. Empty for empty-shaped states.</param>
public sealed record DemoCardListScenarioData(IReadOnlyList<DemoCardItem> Items);
