namespace BeeDayLab.Web.Scenarios;

/// <summary>
/// Illustrative, demo-only implementation of <see cref="IScenarioProvider{TData}"/> (Sprint 33.10)
/// proving the extension point works end to end against Design System primitives already in the
/// Lab from Sprint 33.8 (<c>BeeDayEmptyState</c>, <c>BeeDaySkeleton</c>/<c>BeeDayDashboardSkeleton</c>,
/// <c>BeeDayCard</c> — a caller would render a <c>BeeDayCard</c> per <see cref="DemoCardItem"/>, an
/// empty-state for the empty/no-results shapes, and a skeleton instead of calling this provider at
/// all for <see cref="ScenarioState.Loading"/>/<see cref="ScenarioState.Error"/>, per the
/// "Loading/Error convention" documented on <see cref="IScenarioProvider{TData}"/>). No real page
/// consumes this yet — that is Sprint 33.16/33.17's Component Gallery. Registered as a singleton in
/// Program.cs: it is stateless/pure (holds only immutable static sample data), unlike
/// <see cref="ScenarioSelection"/>, which is per-circuit state and stays <c>Scoped</c>.
///
/// Deterministic per <see cref="IScenarioProvider{TData}.GetScenario"/>'s contract: every branch
/// below returns the *same* cached, static, read-only sample list instance on every call — no
/// <see cref="Random"/>, no <see cref="Guid.NewGuid()"/>, no wall-clock reads.
/// </summary>
public sealed class DemoCardListScenarioProvider : IScenarioProvider<DemoCardListScenarioData>
{
    private static readonly IReadOnlyList<DemoCardItem> EmptyItems = [];

    private static readonly IReadOnlyList<DemoCardItem> PopulatedItems =
    [
        new("Morning stretch routine", "5 minutes, no equipment"),
        new("Hydration check-in", "Log today's water intake"),
        new("Evening journal", "Reflect on three wins from today"),
        new("Deep work block", "90 minutes, notifications muted"),
        new("Walk the dog", "Around the block, 20 minutes"),
        new("Read one chapter", "Continue the book on the nightstand")
    ];

    private static readonly IReadOnlyList<DemoCardItem> LargeContentItems = BuildLargeContentItems();

    /// <inheritdoc />
    public DemoCardListScenarioData GetScenario(ScenarioContext context) => context.State switch
    {
        ScenarioState.Empty => new DemoCardListScenarioData(EmptyItems),
        ScenarioState.NoResults => new DemoCardListScenarioData(EmptyItems),
        ScenarioState.Loading => new DemoCardListScenarioData(EmptyItems),
        ScenarioState.Error => new DemoCardListScenarioData(EmptyItems),
        ScenarioState.Populated => new DemoCardListScenarioData(PopulatedItems),
        ScenarioState.Disabled => new DemoCardListScenarioData(PopulatedItems),
        ScenarioState.Selected => new DemoCardListScenarioData(PopulatedItems),
        ScenarioState.LargeContent => new DemoCardListScenarioData(LargeContentItems),

        // Every named ScenarioState value above is handled explicitly; this only guards an
        // out-of-range value cast from an int, which the enum's public contract does not prevent.
        _ => new DemoCardListScenarioData(EmptyItems)
    };

    private static IReadOnlyList<DemoCardItem> BuildLargeContentItems()
    {
        var items = new List<DemoCardItem>(60);

        for (var index = 1; index <= 60; index++)
        {
            items.Add(new DemoCardItem($"Habit #{index}", $"Day {index} streak entry"));
        }

        return items;
    }
}
