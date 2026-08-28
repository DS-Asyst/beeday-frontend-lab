namespace BeeDayLab.Web.Scenarios;

// ---------------------------------------------------------------------------------------------
// Sprint 33.13 (FE33-088..097) — THE Lab-local presentation contract for the whole Daily/
// productivity surface.
//
// Almost every production file this Sprint extracts is typed against
// BeeDay.Application.Features.Dashboard.Responses (DashboardResponse/UserProfileSummary/
// HabitSummary/TaskSummary/TodoSummary/ProjectSummary) and BeeDay.Domain.Enums — both forbidden in
// the Lab by ADR-008 §2. This file is the ONE translation layer that replaces all of them: every
// page, component, editor model and state class under Components/Pages/Daily/ types against the
// records/enums below and nothing else. It is deliberately NOT a per-page reinvention (Issue #374's
// explicit boundary) and NOT a duplication of business logic — extracting a presentation contract,
// per ADR-008's ADAPT category, is exactly what the Lab is allowed to do.
//
// Every numeric display value that production derives from a Domain/Application calculation
// (ProgressPercentage, TotalExperience, CurrentLevel, CurrentLevelExperience,
// ExperienceRequiredForCurrentLevel) is carried here as an ALREADY-RESOLVED value handed out by
// DailyDashboardScenarioProvider — ADR-008's MOCK category and Issue #374 item 5. Nothing in the
// Lab ever recomputes XP, leveling or project-progress aggregation from parts.
// ---------------------------------------------------------------------------------------------

/// <summary>
/// Lab-local stand-in for <c>BeeDay.Web.Components.Features.Common.ActivityType</c> — which
/// discriminates the four activity kinds a dashboard editor/create action can target.
///
/// <para><b>Ledger correction:</b> the Sprint brief lists this as replacing
/// <c>BeeDay.Domain.Enums.ActivityType</c>, but no such Domain enum exists — production's
/// <c>ActivityType</c> is a Web-layer presentation enum under <c>Components/Features/Common/</c>
/// and carries no Domain coupling of its own. It is still restated here rather than copied to its
/// own file so that this Sprint's whole surface has exactly one enum/record namespace to type
/// against, as Issue #374's boundary requires.</para>
/// </summary>
public enum DailyActivityType
{
    Habit,
    Task,
    Todo,
    Project
}

/// <summary>Lab-local stand-in for <c>BeeDay.Domain.Enums.TaskRepeat</c> (same four members, same order).</summary>
public enum DailyTaskRepeat
{
    None,
    Daily,
    Weekly,
    Monthly
}

/// <summary>Lab-local stand-in for <c>BeeDay.Domain.Enums.ProjectStatus</c> (same three members, same order).</summary>
public enum DailyProjectStatus
{
    Planned,
    InProgress,
    Completed
}

/// <summary>
/// Lab-local stand-in for <c>BeeDay.Domain.Enums.ExperienceSourceType</c>, restricted to the four
/// members the level-up feedback modal actually renders a localized label for
/// (Habit/Task/Todo/Project). Production's enum additionally carries <c>Reading</c>, <c>Manual</c>
/// and <c>System</c>, which <c>BeeDayFeedbackModal</c> only ever reaches through its
/// <c>_ =&gt; source.ToString()</c> fallback and which no Lab flow can produce — the mock level-up
/// trigger is always attributed to one of the four dashboard activity kinds.
/// </summary>
public enum DailyExperienceSource
{
    Habit,
    Task,
    Todo,
    Project
}

/// <summary>
/// Lab-local stand-in for <c>BeeDay.Domain.Enums.ActivityAttribute</c> — an optional productivity
/// classifier carried on every activity editor model. Explicit member values mirror production's.
/// </summary>
public enum DailyActivityAttribute
{
    Strength = 1,
    Dexterity = 2,
    Intelligence = 3,
    Vitality = 4
}

/// <summary>Lab-local stand-in for <c>BeeDay.Domain.Enums.HabitDirection</c>.</summary>
public enum DailyHabitDirection
{
    Positive,
    Negative,
    Both
}

/// <summary>Lab-local stand-in for <c>BeeDay.Domain.Enums.HabitDifficulty</c>.</summary>
public enum DailyHabitDifficulty
{
    Trivial,
    Easy,
    Medium,
    Hard
}

/// <summary>Lab-local stand-in for <c>BeeDay.Domain.Enums.HabitResetCounter</c>.</summary>
public enum DailyHabitResetCounter
{
    Daily,
    Weekly,
    Monthly
}

/// <summary>
/// Lab-local stand-in for <c>BeeDay.Application.Features.Dashboard.Responses.UserProfileSummary</c>.
///
/// <para>Production additionally carries <c>UserLanguage Language</c> and <c>UserTheme Theme</c>.
/// Both are dropped here: they are Domain enums (already given Lab-local equivalents by Sprint
/// 33.12's <c>AccountLanguage</c>/<c>AccountTheme</c> for the Account surface that actually renders
/// them), and no file in this Sprint's Daily surface reads either one.</para>
///
/// <para><see cref="TotalExperience"/>, <see cref="CurrentLevel"/>,
/// <see cref="CurrentLevelExperience"/> and <see cref="ExperienceRequiredForCurrentLevel"/> are
/// pre-resolved display values from the scenario — never computed from an XP curve in the Lab.</para>
/// </summary>
public sealed record DailyUserProfileSummary(
    Guid UserId,
    string Nickname,
    string Name,
    string Avatar,
    long TotalExperience,
    int CurrentLevel,
    long CurrentLevelExperience,
    long ExperienceRequiredForCurrentLevel)
{
    /// <summary>Ported verbatim from production's <c>UserProfileSummary.HasProfile</c>.</summary>
    public bool HasProfile => !string.IsNullOrEmpty(Nickname);
}

/// <summary>
/// Lab-local stand-in for <c>BeeDay.Application.Features.Dashboard.Responses.HabitSummary</c>, field
/// for field. <see cref="PositiveCount"/>/<see cref="NegativeCount"/> are what
/// <c>HabitCard</c>/<c>HabitVisualState</c> subtract into the 7-band balance — the subtraction is
/// pure presentation formatting (a copied <c>int -&gt; CSS class</c> function), not a business rule.
/// </summary>
public sealed record DailyHabitSummary(
    Guid Id,
    string Title,
    string Description,
    bool Featured,
    DailyActivityAttribute? Attribute,
    DailyHabitDirection Direction,
    DailyHabitDifficulty Difficulty,
    DailyHabitResetCounter ResetCounter,
    int PositiveCount,
    int NegativeCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

/// <summary>Lab-local stand-in for <c>BeeDay.Application.Features.Dashboard.Responses.TaskSummary</c>, field for field.</summary>
public sealed record DailyTaskSummary(
    Guid Id,
    string Title,
    string Description,
    bool Featured,
    DailyActivityAttribute? Attribute,
    DailyTaskRepeat Repeat,
    bool Completed,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Lab-local stand-in for <c>BeeDay.Application.Features.Dashboard.Responses.TodoSummary</c>, field
/// for field — including <see cref="ProjectId"/>, which backs both the Todos column's project-context
/// filter and the todo editor's project picker.
/// </summary>
public sealed record DailyTodoSummary(
    Guid Id,
    string Title,
    string Description,
    Guid ProjectId,
    bool Featured,
    DateOnly? DueDate,
    DailyActivityAttribute? Attribute,
    bool Completed,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Lab-local stand-in for <c>BeeDay.Application.Features.Dashboard.Responses.ProjectSummary</c>.
///
/// <para><b>Shape verified against production</b> (the Sprint brief asked not to guess): the real
/// <c>DashboardResponse</c> has NO top-level todos list — todos reach the dashboard exclusively
/// NESTED under their project, as <c>IReadOnlyList&lt;TodoSummary&gt; Todos</c> here, while still
/// carrying their own <c>ProjectId</c>. <c>DashboardState.AllTodos</c> is therefore a
/// <c>Projects.SelectMany(p =&gt; p.Todos)</c> flattening, and <c>LabDashboardState</c> reproduces
/// exactly that.</para>
///
/// <para><see cref="ProgressPercentage"/> is a pre-resolved display value: production derives it
/// from a Domain aggregation, so the Lab always renders whatever the scenario supplied and never
/// recalculates it — not even after a local todo toggle (see <c>LabDashboardState</c>).</para>
/// </summary>
public sealed record DailyProjectSummary(
    Guid Id,
    string Name,
    string Description,
    string Color,
    bool Featured,
    DailyActivityAttribute? Attribute,
    DateOnly? ExpectedDate,
    bool Archived,
    DailyProjectStatus Status,
    decimal ProgressPercentage,
    IReadOnlyList<DailyTodoSummary> Todos)
{
    /// <summary>Ported verbatim from production's <c>ProjectSummary.Completed</c>.</summary>
    public bool Completed => Status == DailyProjectStatus.Completed;
}

/// <summary>
/// Presentation-only scenario data for the whole Daily/productivity surface (Sprint 33.13,
/// FE33-088..097) — the Lab-local mirror of production's <c>DashboardResponse</c>, minus its
/// <c>WalletSummaryResponse? Wallet</c> member (the Wallet surface is Sprint 33.14's scope and no
/// file extracted in this Sprint reads it).
/// </summary>
/// <param name="Profile">Pre-resolved profile/experience display values.</param>
/// <param name="Habits">Synthetic habits, seeded to exercise every <c>HabitVisualState</c> band.</param>
/// <param name="Tasks">Synthetic tasks, some completed.</param>
/// <param name="Projects">Synthetic projects, each nesting its own todos exactly as production does.</param>
/// <param name="XpGainPerAction">
/// The fixed, synthetic amount a single "positive" action (registering a habit positive, completing
/// a task or a to-do) adds to the running <c>TotalExperience</c> display value. This is the ONLY XP
/// number in the Lab: there is no curve, no difficulty multiplier and no diffing against a real
/// profile — Issue #374 item 5 and ADR-008's "não recriar cálculo de regra de negócio (XP...)".
/// </param>
public sealed record DailyDashboardScenarioData(
    DailyUserProfileSummary Profile,
    IReadOnlyList<DailyHabitSummary> Habits,
    IReadOnlyList<DailyTaskSummary> Tasks,
    IReadOnlyList<DailyProjectSummary> Projects,
    int XpGainPerAction);
