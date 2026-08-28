namespace BeeDayLab.Web.Scenarios;

/// <summary>
/// Stateless/pure scenario provider (Sprint 33.13, FE33-088..097) for the whole Daily/productivity
/// surface — <c>Singleton</c> registration in Program.cs, same reasoning as every provider Sprint
/// 33.12 registered (a switch over <see cref="ScenarioContext.State"/> returning static synthetic
/// data, no per-circuit state of its own).
///
/// <para><b>Determinism:</b> every id is derived from a fixed integer seed via
/// <see cref="CreateId"/> and every timestamp/date from a fixed reference instant — no
/// <c>Guid.NewGuid()</c>, no <c>Random</c>, no wall clock, as
/// <c>ScenarioAndLocalizationBoundaryTests</c> enforces for this directory.</para>
///
/// <para><b>State mapping:</b></para>
/// <list type="bullet">
/// <item><see cref="ScenarioState.Empty"/> → every collection empty, but the profile still carries a
/// nickname so <c>HasProfile</c> stays <see langword="true"/>: production's pages gate their whole
/// render on <c>HasProfile</c> and redirect to profile creation otherwise, so an empty-nicknamed
/// profile here would preview a redirect rather than the four empty columns this state exists to
/// show.</item>
/// <item><see cref="ScenarioState.Populated"/> → a realistic board across all four activity types,
/// with completed and active items in each, and seven habits whose balances land one in each of
/// <c>HabitVisualState</c>'s seven CSS bands.</item>
/// <item><see cref="ScenarioState.LargeContent"/> → every reorderable collection above
/// <c>BeeDaySortable.VirtualizationThreshold</c> (30), so <c>/daily</c> actually exercises the
/// virtualized code path this Sprint is the first real consumer of.</item>
/// <item><see cref="ScenarioState.NoResults"/>, <see cref="ScenarioState.Disabled"/> and
/// <see cref="ScenarioState.Selected"/> → the populated board. "Filtered to zero" is produced by the
/// page's own search box / project-context filter over real data
/// (<c>LabDashboardState.HabitsFilteredToZero</c> and friends), never by a provider fabricating an
/// empty result — the distinction between "nothing exists" and "the filter matched nothing" is
/// exactly what those flags encode, and it needs populated data underneath to be reachable.</item>
/// <item><see cref="ScenarioState.Loading"/> and <see cref="ScenarioState.Error"/> → the empty
/// placeholder shape. Per <see cref="IScenarioProvider{TData}"/>'s Loading/Error convention these
/// are the caller's concern: <c>DailyHome</c>/<c>ProfileHome</c> check
/// <c>ScenarioSelection.Context.State</c> and render a skeleton or an unavailable panel without ever
/// asking this provider for data.</item>
/// </list>
/// </summary>
public sealed class DailyDashboardScenarioProvider : IScenarioProvider<DailyDashboardScenarioData>
{
    /// <summary>
    /// The single, fixed, synthetic XP amount every "positive" action awards. Deliberately a flat
    /// constant: Issue #374 item 5 and ADR-008 both forbid the Lab reproducing the real XP rule
    /// (difficulty multipliers, streaks, level curve), so a scenario hands back the final display
    /// number and the Lab only ever adds this one value to a running display total.
    /// </summary>
    private const int SyntheticXpGainPerAction = 10;

    // A fixed reference instant/date, per IScenarioProvider's determinism contract ("when a scenario
    // needs a realistic date, use a fixed reference date").
    private static readonly DateTimeOffset ReferenceInstant = new(2026, 1, 15, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly ReferenceDate = new(2026, 1, 15);

    private const string ProjectAccent = "#8056C7";

    private static readonly DailyUserProfileSummary EmptyProfile = new(
        CreateId(0, 0),
        Nickname: "jordan.silva",
        Name: "Jordan Silva",
        Avatar: string.Empty,
        TotalExperience: 0,
        CurrentLevel: 1,
        CurrentLevelExperience: 0,
        ExperienceRequiredForCurrentLevel: 100);

    // Pre-resolved experience display values — the Lab never derives these from an XP curve.
    private static readonly DailyUserProfileSummary PopulatedProfile = EmptyProfile with
    {
        TotalExperience = 4_820,
        CurrentLevel = 12,
        CurrentLevelExperience = 320,
        ExperienceRequiredForCurrentLevel = 750
    };

    private static readonly DailyDashboardScenarioData EmptyData = new(
        EmptyProfile, [], [], [], SyntheticXpGainPerAction);

    private static readonly DailyDashboardScenarioData PopulatedData = BuildPopulated();

    private static readonly DailyDashboardScenarioData LargeContentData = BuildLargeContent();

    /// <inheritdoc />
    public DailyDashboardScenarioData GetScenario(ScenarioContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.State switch
        {
            ScenarioState.Empty => EmptyData,
            ScenarioState.Populated => PopulatedData,
            ScenarioState.NoResults => PopulatedData,
            ScenarioState.Disabled => PopulatedData,
            ScenarioState.Selected => PopulatedData,
            ScenarioState.LargeContent => LargeContentData,
            _ => EmptyData
        };
    }

    /// <summary>
    /// Builds a stable <see cref="Guid"/> from two small integer seeds — a deterministic replacement
    /// for <c>Guid.NewGuid()</c>, which this directory's architecture guard forbids.
    /// </summary>
    /// <param name="kind">Collection discriminator (1 habit, 2 task, 3 todo, 4 project).</param>
    /// <param name="index">Position within that collection.</param>
    private static Guid CreateId(int kind, int index) =>
        new(index, (short)kind, 0, [0, 0, 0, 0, 0, 0, 0, 0]);

    private static DailyDashboardScenarioData BuildPopulated()
    {
        // Balances chosen so the seven habits land one in each HabitVisualState band:
        // 24 -> sky (>=21), 16 -> green (>=14), 9 -> yellow (>=7), 0 -> white (the neutral default),
        // -3 -> red-light (<=-1), -9 -> red-medium (<=-7), -18 -> red-strong (<=-14).
        DailyHabitSummary Habit(
            int index, string title, string description, int positive, int negative,
            DailyHabitDirection direction, DailyHabitDifficulty difficulty, bool featured) =>
            new(CreateId(1, index), title, description, featured, DailyActivityAttribute.Vitality,
                direction, difficulty, DailyHabitResetCounter.Daily, positive, negative,
                ReferenceInstant, ReferenceInstant);

        List<DailyHabitSummary> habits =
        [
            Habit(1, "Morning water", "A full glass before anything else.", 30, 6, DailyHabitDirection.Positive, DailyHabitDifficulty.Trivial, true),
            Habit(2, "Read 20 pages", "Any book, paper or long-form article.", 22, 6, DailyHabitDirection.Positive, DailyHabitDifficulty.Easy, false),
            Habit(3, "Walk after lunch", "Fifteen minutes outside, no phone.", 14, 5, DailyHabitDirection.Both, DailyHabitDifficulty.Easy, false),
            Habit(4, "Inbox to zero", "Clear the inbox before the day ends.", 11, 11, DailyHabitDirection.Both, DailyHabitDifficulty.Medium, false),
            Habit(5, "Late-night snacking", "Nothing after nine in the evening.", 4, 7, DailyHabitDirection.Negative, DailyHabitDifficulty.Medium, false),
            Habit(6, "Doomscrolling", "Feeds after bedtime count against this.", 3, 12, DailyHabitDirection.Negative, DailyHabitDifficulty.Hard, false),
            Habit(7, "Skipping the gym", "Every skipped session lands here.", 2, 20, DailyHabitDirection.Negative, DailyHabitDifficulty.Hard, true)
        ];

        DailyTaskSummary Task(int index, string title, string description, DailyTaskRepeat repeat, bool completed, bool featured) =>
            new(CreateId(2, index), title, description, featured, DailyActivityAttribute.Intelligence,
                repeat, completed, ReferenceInstant, ReferenceInstant);

        List<DailyTaskSummary> tasks =
        [
            Task(1, "Daily stand-up", "Fifteen minutes with the team.", DailyTaskRepeat.Daily, false, true),
            Task(2, "Review pull requests", "Clear the review queue.", DailyTaskRepeat.Daily, false, false),
            Task(3, "Weekly planning", "Set the week's three priorities.", DailyTaskRepeat.Weekly, false, false),
            Task(4, "Pay the invoices", "Recurring bills and subscriptions.", DailyTaskRepeat.Monthly, false, false),
            Task(5, "Back up the laptop", "Full snapshot to the external drive.", DailyTaskRepeat.Weekly, true, false),
            Task(6, "Renew the domain", "One-off, no repeat.", DailyTaskRepeat.None, true, false)
        ];

        DailyTodoSummary Todo(int index, Guid projectId, string title, string description, bool completed, DateOnly? dueDate, bool featured) =>
            new(CreateId(3, index), title, description, projectId, featured, dueDate,
                DailyActivityAttribute.Dexterity, completed, ReferenceInstant, ReferenceInstant);

        var apartmentId = CreateId(4, 1);
        var courseId = CreateId(4, 2);
        var marathonId = CreateId(4, 3);

        List<DailyProjectSummary> projects =
        [
            new(apartmentId, "Apartment refresh", "Repaint, rewire and rearrange the flat.", ProjectAccent,
                Featured: true, DailyActivityAttribute.Strength, ReferenceDate.AddDays(45), Archived: false,
                DailyProjectStatus.InProgress, ProgressPercentage: 62.5m,
                [
                    Todo(1, apartmentId, "Pick the paint colours", "Two neutrals and one accent.", true, ReferenceDate.AddDays(3), false),
                    Todo(2, apartmentId, "Order the shelving", "Measure the alcove twice first.", true, ReferenceDate.AddDays(10), false),
                    Todo(3, apartmentId, "Book the electrician", "Kitchen sockets need moving.", false, ReferenceDate.AddDays(21), true),
                    Todo(4, apartmentId, "Rehang the curtains", "After the walls are dry.", false, null, false)
                ]),
            new(courseId, "Finish the design course", "Twelve modules, one certificate.", ProjectAccent,
                Featured: false, DailyActivityAttribute.Intelligence, ReferenceDate.AddDays(90), Archived: false,
                DailyProjectStatus.Planned, ProgressPercentage: 25m,
                [
                    Todo(5, courseId, "Watch modules 1-3", "Take notes as you go.", true, ReferenceDate.AddDays(7), false),
                    Todo(6, courseId, "Submit the first critique", "Peer review, 300 words.", false, ReferenceDate.AddDays(14), false),
                    Todo(7, courseId, "Build the capstone brief", "Pick a real problem to solve.", false, null, false)
                ]),
            new(marathonId, "Half marathon", "Twelve-week build to race day.", ProjectAccent,
                Featured: false, DailyActivityAttribute.Vitality, ReferenceDate.AddDays(-5), Archived: false,
                DailyProjectStatus.Completed, ProgressPercentage: 100m,
                [
                    Todo(8, marathonId, "Long run: 18 km", "Easy pace, negative split.", true, ReferenceDate.AddDays(-30), false),
                    Todo(9, marathonId, "Race day", "Start slow, finish strong.", true, ReferenceDate.AddDays(-5), true)
                ])
        ];

        return new DailyDashboardScenarioData(
            PopulatedProfile, habits, tasks, projects, SyntheticXpGainPerAction);
    }

    private static DailyDashboardScenarioData BuildLargeContent()
    {
        // BeeDaySortable virtualizes at ItemIds.Count >= VirtualizationThreshold (30). Every one of
        // the four sortable collections on /daily is seeded past that: habits, ACTIVE tasks, ACTIVE
        // todos and ACTIVE projects — the counts below are the active ones, not the totals.
        const int HabitCount = 36;
        const int ActiveTaskCount = 36;
        const int CompletedTaskCount = 6;
        const int ActiveProjectCount = 34;
        const int CompletedProjectCount = 4;

        var habits = Enumerable.Range(1, HabitCount)
            .Select(index => new DailyHabitSummary(
                CreateId(1, index),
                $"Habit {index:00}",
                $"Synthetic habit {index:00} for the large-content preview.",
                Featured: index % 9 == 0,
                DailyActivityAttribute.Vitality,
                // Cycles all three directions so both score buttons' visibility is exercised.
                (DailyHabitDirection)(index % 3),
                (DailyHabitDifficulty)(index % 4),
                DailyHabitResetCounter.Daily,
                // Spreads balances from -18 to +17, covering every HabitVisualState band again.
                PositiveCount: index,
                NegativeCount: 18,
                ReferenceInstant,
                ReferenceInstant))
            .ToList();

        var tasks = Enumerable.Range(1, ActiveTaskCount + CompletedTaskCount)
            .Select(index => new DailyTaskSummary(
                CreateId(2, index),
                $"Task {index:00}",
                $"Synthetic task {index:00} for the large-content preview.",
                Featured: index % 11 == 0,
                DailyActivityAttribute.Intelligence,
                (DailyTaskRepeat)(index % 4),
                Completed: index > ActiveTaskCount,
                ReferenceInstant,
                ReferenceInstant))
            .ToList();

        var projects = Enumerable.Range(1, ActiveProjectCount + CompletedProjectCount)
            .Select(index =>
            {
                var projectId = CreateId(4, index);
                var isCompletedProject = index > ActiveProjectCount;

                // One todo per project, flattened by LabDashboardState into a single Todos column;
                // the last two are completed, leaving 36 active todos — also past the threshold.
                var isCompletedTodo = index > ActiveProjectCount + CompletedProjectCount - 2;

                return new DailyProjectSummary(
                    projectId,
                    $"Project {index:00}",
                    $"Synthetic project {index:00} for the large-content preview.",
                    ProjectAccent,
                    Featured: index % 13 == 0,
                    DailyActivityAttribute.Strength,
                    ReferenceDate.AddDays(index),
                    Archived: false,
                    isCompletedProject ? DailyProjectStatus.Completed
                        : index % 2 == 0 ? DailyProjectStatus.InProgress
                        : DailyProjectStatus.Planned,
                    // Pre-resolved display percentages, never derived from the todo counts below.
                    ProgressPercentage: isCompletedProject ? 100m : index * 2m,
                    [
                        new DailyTodoSummary(
                            CreateId(3, index),
                            $"To-do {index:00}",
                            $"Synthetic to-do {index:00} for the large-content preview.",
                            projectId,
                            Featured: index % 17 == 0,
                            ReferenceDate.AddDays(index),
                            DailyActivityAttribute.Dexterity,
                            Completed: isCompletedTodo,
                            ReferenceInstant,
                            ReferenceInstant)
                    ]);
            })
            .ToList();

        var profile = PopulatedProfile with
        {
            TotalExperience = 91_400,
            CurrentLevel = 47,
            CurrentLevelExperience = 1_180,
            ExperienceRequiredForCurrentLevel = 2_400
        };

        return new DailyDashboardScenarioData(
            profile, habits, tasks, projects, SyntheticXpGainPerAction);
    }
}
