namespace BeeDayLab.Web.Scenarios;

/// <summary>
/// The named presentation states every scenario provider (Sprint 33.10, FE33-104, Issue #371) may
/// be asked to render. Fixed to the 8 states the issue enumerates — "disabled/read-only" maps to
/// <see cref="Disabled"/>, "selected/completed" maps to <see cref="Selected"/>. A provider is not
/// required to give every state a visually distinct result (e.g. a list-shaped provider may
/// legitimately treat <see cref="NoResults"/> the same as <see cref="Empty"/>), but every provider
/// must still handle every value here — see <see cref="IScenarioProvider{TData}"/>.
/// </summary>
public enum ScenarioState
{
    /// <summary>No data exists yet (first-run, nothing created).</summary>
    Empty,

    /// <summary>The common case: a realistic, non-trivial amount of synthetic data.</summary>
    Populated,

    /// <summary>
    /// In flight. By convention the caller checks <c>context.State</c> and renders a skeleton
    /// (e.g. <c>BeeDaySkeleton</c>/<c>BeeDayDashboardSkeleton</c>) directly, without asking a
    /// provider for data at all — see the "Loading/Error convention" remarks on
    /// <see cref="IScenarioProvider{TData}"/>.
    /// </summary>
    Loading,

    /// <summary>
    /// The last operation failed. Same caller-handles-it convention as <see cref="Loading"/>.
    /// </summary>
    Error,

    /// <summary>Data exists, but the active filter/search produced nothing — distinct narrative from <see cref="Empty"/>.</summary>
    NoResults,

    /// <summary>Populated, but read-only/non-interactive.</summary>
    Disabled,

    /// <summary>A large, stress-test volume of data (pagination, virtualization, overflow).</summary>
    LargeContent,

    /// <summary>An item (or the whole scenario) is selected/completed.</summary>
    Selected
}
