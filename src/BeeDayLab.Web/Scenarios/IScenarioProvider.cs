namespace BeeDayLab.Web.Scenarios;

/// <summary>
/// The single extension point every later feature Sprint (Public/Identity/Daily/Wallet/Email —
/// Sprints 33.11-33.15) plugs into instead of inventing its own page-local mock mechanism.
/// ADR-008's "ADAPT" and "MOCK" categories both resolve to this: a component that today injects a
/// real service, or whose visual state depends on a Domain/Application calculation, gets its
/// presentation contract extracted into a small feature-owned data model, and a provider of this
/// interface hands out pre-resolved, realistic synthetic values for it — the Lab never recalculates
/// the business rule itself.
///
/// <para><b>Pattern a later Sprint follows to plug in:</b></para>
/// <list type="number">
/// <item>Define a small, presentation-only data model for the feature, e.g.
/// <c>WalletScenarioData</c> — only primitives/records the Lab is allowed to use, never a
/// <c>BeeDay.Domain</c>/<c>BeeDay.Application</c> type.</item>
/// <item>Implement this interface once per feature:
/// <c>WalletScenarioProvider : IScenarioProvider&lt;WalletScenarioData&gt;</c>.</item>
/// <item><see cref="GetScenario"/> switches over <see cref="ScenarioContext.State"/> and returns a
/// different, realistic, synthetic <c>WalletScenarioData</c> instance per
/// <see cref="ScenarioState"/> value.</item>
/// </list>
///
/// <code>
/// public sealed record WalletScenarioData(decimal Balance, IReadOnlyList&lt;string&gt; RecentTransactions);
///
/// public sealed class WalletScenarioProvider : IScenarioProvider&lt;WalletScenarioData&gt;
/// {
///     public WalletScenarioData GetScenario(ScenarioContext context) => context.State switch
///     {
///         ScenarioState.Empty =&gt; new WalletScenarioData(0m, []),
///         ScenarioState.Populated =&gt; new WalletScenarioData(128.50m, SampleTransactions),
///         ScenarioState.LargeContent =&gt; new WalletScenarioData(4_812.00m, ManyTransactions),
///         _ =&gt; new WalletScenarioData(0m, [])
///     };
/// }
/// </code>
///
/// <para><b>Determinism (enforced by Sprint 33.10's ScenarioEngineTests, and expected of every
/// later Sprint's own provider tests too):</b> <see cref="GetScenario"/> MUST be a pure function of
/// its <see cref="ScenarioContext"/> argument — the same context value must always produce an
/// equal <typeparamref name="TData"/> result, across calls, across the process lifetime. Never use
/// <see cref="Random"/>, an unseeded <see cref="Guid.NewGuid"/>, <see cref="DateTime.Now"/>, or
/// <see cref="DateTimeOffset.UtcNow"/> inside an implementation. When a scenario needs a
/// "realistic" date, use a fixed reference date instead (e.g. <c>new DateOnly(2026, 1, 15)</c>).
/// </para>
///
/// <para><b>Loading/Error convention:</b> <see cref="ScenarioState.Loading"/> and
/// <see cref="ScenarioState.Error"/> are typically handled by the *caller* — checking
/// <see cref="ScenarioContext.State"/> before it even asks a provider for data, and rendering a
/// skeleton (<c>BeeDaySkeleton</c>/<c>BeeDayDashboardSkeleton</c>) or an error state
/// (<c>BeeDayEmptyState</c>, an inline error banner) directly — rather than a provider fabricating
/// fake error content. A provider may still return an empty/placeholder shape for these two states
/// purely so <see cref="GetScenario"/> stays a total function over every
/// <see cref="ScenarioState"/> value; callers should not treat that placeholder as meaningful data.
/// </para>
/// </summary>
/// <typeparam name="TData">The feature's presentation-only scenario data model.</typeparam>
public interface IScenarioProvider<TData>
{
    /// <summary>Returns the realistic synthetic data for <paramref name="context"/>. Must be a pure function — see remarks.</summary>
    TData GetScenario(ScenarioContext context);
}
