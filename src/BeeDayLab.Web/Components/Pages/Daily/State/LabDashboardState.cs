using BeeDayLab.Web.Components.Behaviors.DragDrop;
using BeeDayLab.Web.Components.DesignSystem.Feedback;
using BeeDayLab.Web.Components.Pages.Daily.Experience.Feedback;
using BeeDayLab.Web.Components.Pages.Daily.Habits.Models;
using BeeDayLab.Web.Components.Pages.Daily.Projects.Models;
using BeeDayLab.Web.Components.Pages.Daily.Tasks.Models;
using BeeDayLab.Web.Components.Pages.Daily.Todos.Models;
using BeeDayLab.Web.Scenarios;
using Microsoft.Extensions.Localization;

namespace BeeDayLab.Web.Components.Pages.Daily.State;

/// <summary>
/// ADAPT (Sprint 33.13, FE33-090) of BeeDay.Web's
/// <c>Components/Features/Dashboard/State/DashboardState.cs</c> — the densest single file in this
/// Sprint, and the one that decides what the whole Daily surface can preview. Registered
/// <c>Scoped</c> in Program.cs for the same reason production registers its counterpart (and the same
/// reason <c>ProfileCreationState</c>/<c>ToastService</c>/<c>ScenarioSelection</c> are Scoped): this
/// is per-circuit UI state with a cancellation token tied to the circuit's lifetime.
///
/// <para><b>Preserved verbatim</b> — the entire client-side search/filter/counting surface, which is
/// pure presentation logic over in-memory collections with zero backend dependency, and which Issue
/// #374 item 4 explicitly requires be kept faithful: <see cref="Search"/>, <see cref="ClearFilters"/>,
/// <see cref="SelectProjectContext"/>/<see cref="SelectedProjectId"/>/<see cref="ProjectContextOptions"/>,
/// <c>Filter</c>/<c>MatchesSearch</c>, the four <c>FilteredX</c> projections, the
/// <see cref="CompletedItems"/>/<see cref="ActiveItems"/>/<see cref="TotalItems"/>/<see cref="FilteredItems"/>
/// counters, the four <c>XFilteredToZero</c> flags (which distinguish "the filter matched nothing"
/// from "this collection is genuinely empty"), <see cref="FormatRepeat"/>/<see cref="FormatDueDate"/>/
/// <see cref="FormatProjectStatus"/>, <see cref="Modals"/>, and the project-workspace open/close pair.
/// The <see cref="ExecuteAsync"/> busy-guard/toast wrapper and the <see cref="RemovingItemId"/>
/// delete-animation handshake are ported unchanged too.</para>
///
/// <para><b>Data source replaced</b> — production's <c>store.LoadDashboardAsync(...)</c>
/// (<c>BeeDayWebService</c> → MediatR → Application → EF Core) is replaced by a single
/// <c>DailyDashboardScenarioProvider.GetScenario(...)</c> call in <see cref="InitializeAsync"/>,
/// which seeds a mutable in-memory working copy. There is no reload path at all: every mutation
/// below edits that working copy directly.</para>
///
/// <para><b>Every mutation replaced</b> — the <c>store.XxxAsync(...)</c> + <c>await ReloadAsync()</c>
/// pair becomes a short synthetic delay (preserving the busy-state UX and the loading overlay) then
/// a direct local add/update/remove/toggle plus <c>Changed?.Invoke()</c>. This is the "inert/local
/// state" Issue #374 item 3 asks for: dialogs mutate memory only, nothing is persisted, and no
/// Application handler exists to call. Reordering still runs the real, already-extracted
/// <see cref="SortableOrder.Move"/> (pure presentation logic) and then applies the resulting order to
/// the local list instead of calling <c>store.ReorderAsync</c>.</para>
///
/// <para><b>Internal shape note</b> — the scenario/DTO contract nests todos inside their project
/// exactly as production's <c>DashboardResponse</c> does (see <see cref="DailyProjectSummary"/>).
/// This class flattens them into one <c>todos</c> list on seed and re-nests them on demand
/// (<see cref="OpenProject"/>), purely so that all four collections share one uniform
/// mutate/reorder implementation. The flattening mirrors production's own
/// <c>AllTodos = Projects.SelectMany(p =&gt; p.Todos)</c>, and nothing outside this class observes the
/// difference.</para>
/// </summary>
public sealed class LabDashboardState : IDisposable
{
    /// <summary>
    /// Stands in for the round-trip latency of production's real save/delete/toggle/reorder calls, so
    /// the busy overlay, the disabled editor buttons and the delete animation all still have
    /// something to render against. Long enough to be visible, short enough not to feel broken.
    /// </summary>
    private const int MutationDelayMilliseconds = 200;

    /// <summary>Production's own pre-delete animation window, ported unchanged.</summary>
    private const int RemovalAnimationMilliseconds = 170;

    /// <summary>Production's own experience-gain display window, ported unchanged.</summary>
    private const int ExperienceFeedbackMilliseconds = 750;

    /// <summary>
    /// <b>Mock level-up trigger (FE33-096).</b> Production raises the level-up modal from a real
    /// <c>UserLeveledUpDomainEvent</c> travelling through MediatR into
    /// <c>BeeDayFeedbackEventHandler</c> — a pipeline the Lab must not reproduce. Instead, every Nth
    /// positive-experience action (registering a habit positive, or completing a task/to-do) calls
    /// <c>BeeDayFeedbackStore.Add(...)</c> directly with synthetic level-up data.
    ///
    /// <para>Chosen deliberately to be <b>deterministic and discoverable</b>: from a freshly loaded
    /// <c>/daily</c>, completing any three items in a row raises the modal — no hidden thresholds, no
    /// scenario-state gate to hunt for, and reproducible in a test by calling three toggles. Three is
    /// small enough to reach by accident while exploring the page, which is the point of a visual
    /// lab.</para>
    /// </summary>
    private const int LevelUpEveryNthPositiveAction = 3;

    private readonly ToastService toastService;
    private readonly ScenarioSelection scenarioSelection;
    private readonly DailyDashboardScenarioProvider scenarioProvider;
    private readonly BeeDayFeedbackStore feedbackStore;
    private readonly IStringLocalizer<DashboardResources> localizer;

    // Scoped to this circuit's lifetime (this type is registered Scoped) — cancels the in-flight
    // synthetic delays this instance started once the circuit ends, exactly as production cancels
    // its in-flight queries/mutations.
    private readonly CancellationTokenSource cancellation = new();

    // The mutable in-memory working copy seeded from the scenario. This IS the Lab's "persistence":
    // local, inert, per-circuit, discarded on reload.
    private readonly List<DailyHabitSummary> habits = [];
    private readonly List<DailyTaskSummary> tasks = [];
    private readonly List<DailyTodoSummary> todos = [];
    private readonly List<DailyProjectSummary> projects = [];

    private DailyUserProfileSummary profile = EmptyProfile;
    private int xpGainPerAction;
    private long totalExperience;
    private int currentLevel = 1;
    private int positiveExperienceActionCount;

    private bool isInitialized;
    private string search = string.Empty;
    private Guid? selectedProjectId;
    private Task? initializationTask;

    private static readonly DailyUserProfileSummary EmptyProfile =
        new(Guid.Empty, string.Empty, string.Empty, string.Empty, 0, 1, 0, 0);

    public LabDashboardState(
        ToastService toastService,
        ScenarioSelection scenarioSelection,
        DailyDashboardScenarioProvider scenarioProvider,
        BeeDayFeedbackStore feedbackStore,
        IStringLocalizer<DashboardResources> localizer)
    {
        this.toastService = toastService;
        this.scenarioSelection = scenarioSelection;
        this.scenarioProvider = scenarioProvider;
        this.feedbackStore = feedbackStore;
        this.localizer = localizer;
    }

    public void Dispose()
    {
        cancellation.Cancel();
        cancellation.Dispose();
    }

    public DashboardModalState Modals { get; } = new();

    public event Action? Changed;

    // ---------------------------------------------------------------------------------------------
    // Data surface. Production exposes a single nullable DashboardResponse; the Lab exposes the four
    // collections plus the profile directly, since there is no response object to hand back.
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    /// The profile, with <see cref="DailyUserProfileSummary.TotalExperience"/> and
    /// <see cref="DailyUserProfileSummary.CurrentLevel"/> overlaid from the running local display
    /// values (see <see cref="AddExperienceForPositiveAction"/>). Every other field is the
    /// scenario's, untouched.
    /// </summary>
    public DailyUserProfileSummary Profile => profile with
    {
        TotalExperience = totalExperience,
        CurrentLevel = currentLevel
    };

    public IReadOnlyList<DailyHabitSummary> Habits => habits;
    public IReadOnlyList<DailyTaskSummary> Tasks => tasks;
    public IReadOnlyList<DailyProjectSummary> Projects => projects;

    /// <summary>Production's <c>DashboardState.IsLoading</c> (<c>data is null</c>) equivalent.</summary>
    public bool IsLoading => !isInitialized;

    public bool IsUnavailable { get; private set; }
    public bool IsBusy { get; private set; }
    public long LatestExperienceGain { get; private set; }
    public long ExperienceFeedbackVersion { get; private set; }
    public Guid? RemovingItemId { get; private set; }

    public bool HasProfile => isInitialized && Profile.HasProfile;

    public Guid? OpenProjectId { get; private set; }

    /// <summary>
    /// The currently open project, re-nesting its todos so <c>ProjectWorkspace</c> receives the same
    /// shape production hands it. Re-derived on every read, exactly like production's own
    /// <c>OpenProject</c>.
    /// </summary>
    public DailyProjectSummary? OpenProject => OpenProjectId is Guid id
        ? projects.FirstOrDefault(project => project.Id == id) is DailyProjectSummary project
            ? project with { Todos = todos.Where(todo => todo.ProjectId == project.Id).ToList() }
            : null
        : null;

    public string Search
    {
        get => search;
        set => search = value ?? string.Empty;
    }

    public Guid? SelectedProjectId => selectedProjectId;

    public IReadOnlyList<DailyProjectSummary> ProjectContextOptions => projects.ToList();

    public void SelectProjectContext(Guid? projectId)
    {
        selectedProjectId = projectId is Guid id && projects.Any(project => project.Id == id)
            ? id
            : null;
        Changed?.Invoke();
    }

    // A single reset for every filter that can narrow the board to zero — search text and the
    // Todos-only project context — so a column's "clear filter" action always recovers fully,
    // regardless of which one caused the empty result.
    public void ClearFilters()
    {
        search = string.Empty;
        selectedProjectId = null;
        Changed?.Invoke();
    }

    private IEnumerable<DailyTodoSummary> AllTodos => todos;

    public int CompletedItems => !isInitialized
        ? 0
        : tasks.Count(item => item.Completed)
          + AllTodos.Count(item => item.Completed)
          + projects.Count(item => item.Completed);

    public int ActiveItems => !isInitialized
        ? 0
        : habits.Count
          + tasks.Count(item => !item.Completed)
          + AllTodos.Count(item => !item.Completed)
          + projects.Count(item => !item.Completed);

    public IEnumerable<DailyHabitSummary> FilteredHabits =>
        Filter(habits, item => item.Title, item => item.Description);

    public IEnumerable<DailyTaskSummary> FilteredTasks =>
        Filter(tasks, item => item.Title, item => item.Description);

    public IEnumerable<DailyTodoSummary> FilteredTodos =>
        Filter(AllTodos, item => item.Title, item => item.Description)
            .Where(item => selectedProjectId is null || item.ProjectId == selectedProjectId);

    public IEnumerable<DailyProjectSummary> FilteredProjects =>
        Filter(projects, item => item.Name, item => item.Description);

    public int TotalItems => !isInitialized
        ? 0
        : habits.Count + tasks.Count + AllTodos.Count() + projects.Count;

    public int FilteredItems =>
        FilteredHabits.Count() + FilteredTasks.Count() + FilteredTodos.Count() + FilteredProjects.Count();

    private bool IsSearchActive => !string.IsNullOrWhiteSpace(search);

    // Distinguishes "the search/filter narrowed an active collection to zero" from "this collection
    // genuinely never had any active items" — the two would otherwise render identical empty-state
    // text, so a user searching an unmatched term could wrongly conclude they had nothing at all.
    public bool HabitsFilteredToZero =>
        IsSearchActive && !FilteredHabits.Any() && habits.Count > 0;

    public bool ActiveTasksFilteredToZero =>
        IsSearchActive && !FilteredTasks.Any(item => !item.Completed) && tasks.Count(item => !item.Completed) > 0;

    public bool ActiveTodosFilteredToZero =>
        (IsSearchActive || selectedProjectId is not null)
            && !FilteredTodos.Any(item => !item.Completed)
            && AllTodos.Count(item => !item.Completed) > 0;

    public bool ActiveProjectsFilteredToZero =>
        IsSearchActive && !FilteredProjects.Any(item => !item.Completed) && projects.Count(item => !item.Completed) > 0;

    // ---------------------------------------------------------------------------------------------
    // Initialization — the one place the scenario engine is consulted.
    // ---------------------------------------------------------------------------------------------

    public Task InitializeAsync() => initializationTask ??= InitializeCoreAsync();

    private Task InitializeCoreAsync()
    {
        var context = scenarioSelection.Context;

        // Per IScenarioProvider's Loading/Error convention these two states are the caller's concern,
        // handled here rather than by the provider fabricating fake content.
        if (context.State == ScenarioState.Loading)
        {
            // Deliberately never completes initialization: IsLoading stays true so the page keeps
            // rendering BeeDayDashboardSkeleton, which is exactly what this state exists to preview.
            return Task.CompletedTask;
        }

        if (context.State == ScenarioState.Error)
        {
            // Mirrors production's catch-block: an error toast, an empty board, IsUnavailable set.
            toastService.ShowError(localizer["DashboardLoadErrorMessage"]);
            Seed(new DailyDashboardScenarioData(EmptyProfile, [], [], [], 0));
            IsUnavailable = true;
            Changed?.Invoke();
            return Task.CompletedTask;
        }

        Seed(scenarioProvider.GetScenario(context));
        IsUnavailable = false;
        Changed?.Invoke();
        return Task.CompletedTask;
    }

    private void Seed(DailyDashboardScenarioData scenario)
    {
        profile = scenario.Profile;
        totalExperience = scenario.Profile.TotalExperience;
        currentLevel = scenario.Profile.CurrentLevel;
        xpGainPerAction = scenario.XpGainPerAction;

        habits.Clear();
        habits.AddRange(scenario.Habits);

        tasks.Clear();
        tasks.AddRange(scenario.Tasks);

        // Flattened exactly as production's AllTodos does; the nested shape is restored on demand.
        todos.Clear();
        todos.AddRange(scenario.Projects.SelectMany(project => project.Todos));

        projects.Clear();
        projects.AddRange(scenario.Projects.Select(project => project with { Todos = [] }));

        isInitialized = true;
    }

    // ---------------------------------------------------------------------------------------------
    // Modal/workspace surface — ported verbatim.
    // ---------------------------------------------------------------------------------------------

    public void OpenCreate(DailyActivityType type) => Modals.OpenCreate(type);
    public void OpenHabitEditor(DailyHabitSummary item) => Modals.OpenHabit(item);
    public void OpenTaskEditor(DailyTaskSummary item) => Modals.OpenTask(item);
    public void OpenTodoEditor(DailyTodoSummary item) => Modals.OpenTodo(item);
    public void OpenProjectEditor(DailyProjectSummary item) => Modals.OpenProject(item);

    public void OpenProjectWorkspace(DailyProjectSummary item)
    {
        ArgumentNullException.ThrowIfNull(item);

        OpenProjectId = item.Id;
        Changed?.Invoke();
    }

    public void OpenProjectFromEditor()
    {
        if (Modals.EditingId is Guid id
            && projects.FirstOrDefault(project => project.Id == id) is DailyProjectSummary project)
        {
            Modals.CloseEditor();
            OpenProjectWorkspace(project);
        }
    }

    public void CloseProjectWorkspace()
    {
        OpenProjectId = null;
        Changed?.Invoke();
    }

    public void OpenTodoForProject()
    {
        if (OpenProjectId is Guid projectId)
        {
            Modals.OpenTodoForProject(projectId);
            Changed?.Invoke();
        }
    }

    public void CloseEditor() => Modals.CloseEditor();

    // ---------------------------------------------------------------------------------------------
    // Save — local, in-memory only. Production's store.Add/Update + ReloadAsync pair is replaced by
    // the synthetic delay + a direct list edit.
    // ---------------------------------------------------------------------------------------------

    public Task SaveHabitAsync(HabitEditorModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var isEditing = Modals.IsEditing;
        return SaveEditorAsync(
            () => UpsertHabit(model),
            isEditing ? localizer["HabitUpdatedMessage"] : localizer["HabitCreatedMessage"]);
    }

    public Task SaveTaskAsync(TaskEditorModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var isEditing = Modals.IsEditing;
        return SaveEditorAsync(
            () => UpsertTask(model),
            isEditing ? localizer["TaskUpdatedMessage"] : localizer["TaskCreatedMessage"]);
    }

    public Task SaveTodoAsync(TodoEditorModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var isEditing = Modals.IsEditing;
        return SaveEditorAsync(
            () => UpsertTodo(model),
            isEditing ? localizer["TodoUpdatedMessage"] : localizer["TodoCreatedMessage"]);
    }

    /// <summary>
    /// The project-workspace "add to-do" flow, which production keeps separate from
    /// <see cref="SaveTodoAsync"/> because it always creates (never edits) and carries its own error
    /// message. Ported with the same distinction.
    /// </summary>
    public Task SaveTodoFromProjectAsync(TodoEditorModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return ExecuteAsync(
            async () =>
            {
                await DelayAsync();
                UpsertTodo(model);
                Changed?.Invoke();
            },
            localizer["TodoCreatedMessage"],
            localizer["TodoCreateFromProjectErrorMessage"]);
    }

    public Task SaveProjectAsync(ProjectEditorModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var isEditing = Modals.IsEditing;
        return SaveEditorAsync(
            () => UpsertProject(model),
            isEditing ? localizer["ProjectUpdatedMessage"] : localizer["ProjectCreatedMessage"]);
    }

    private void UpsertHabit(HabitEditorModel model)
    {
        var index = IndexOf(habits, item => item.Id, Modals.EditingId);

        if (index >= 0)
        {
            habits[index] = habits[index] with
            {
                Title = model.Title,
                Description = model.Description,
                Direction = model.Direction,
                Difficulty = model.Difficulty,
                ResetCounter = model.ResetCounter,
                Attribute = model.Attribute
            };
            return;
        }

        // A brand-new item needs an id, and only the local working copy will ever see it. Guid.NewGuid
        // is fine here (unlike inside Scenarios/, where determinism is enforced) precisely because
        // this is user-driven local mutation, not scenario data.
        habits.Add(new DailyHabitSummary(
            Guid.NewGuid(),
            model.Title,
            model.Description,
            Featured: false,
            model.Attribute,
            model.Direction,
            model.Difficulty,
            model.ResetCounter,
            PositiveCount: 0,
            NegativeCount: 0,
            CreatedAtUtc: default,
            UpdatedAtUtc: default));
    }

    private void UpsertTask(TaskEditorModel model)
    {
        var index = IndexOf(tasks, item => item.Id, Modals.EditingId);

        if (index >= 0)
        {
            tasks[index] = tasks[index] with
            {
                Title = model.Title,
                Description = model.Description,
                Repeat = model.Repeat,
                Attribute = model.Attribute
            };
            return;
        }

        tasks.Add(new DailyTaskSummary(
            Guid.NewGuid(),
            model.Title,
            model.Description,
            Featured: false,
            model.Attribute,
            model.Repeat,
            Completed: false,
            CreatedAtUtc: default,
            UpdatedAtUtc: default));
    }

    private void UpsertTodo(TodoEditorModel model)
    {
        var index = IndexOf(todos, item => item.Id, Modals.EditingId);
        var dueDate = model.DueDate is DateTime due ? DateOnly.FromDateTime(due) : (DateOnly?)null;

        if (index >= 0)
        {
            todos[index] = todos[index] with
            {
                Title = model.Title,
                Description = model.Description,
                DueDate = dueDate,
                ProjectId = model.ProjectId ?? todos[index].ProjectId,
                Attribute = model.Attribute
            };
            return;
        }

        todos.Add(new DailyTodoSummary(
            Guid.NewGuid(),
            model.Title,
            model.Description,
            model.ProjectId ?? Guid.Empty,
            Featured: false,
            dueDate,
            model.Attribute,
            Completed: false,
            CreatedAtUtc: default,
            UpdatedAtUtc: default));
    }

    private void UpsertProject(ProjectEditorModel model)
    {
        var index = IndexOf(projects, item => item.Id, Modals.EditingId);

        if (index >= 0)
        {
            projects[index] = projects[index] with
            {
                Name = model.Title,
                Description = model.Description,
                ExpectedDate = model.ExpectedDate is DateTime expected ? DateOnly.FromDateTime(expected) : null,
                Archived = model.Archived,
                Attribute = model.Attribute
            };
            return;
        }

        // ProgressPercentage/Status are display values a scenario normally resolves; a locally created
        // project starts at the only pair the Lab may state without inventing a rule — Planned/0%.
        projects.Add(new DailyProjectSummary(
            Guid.NewGuid(),
            model.Title,
            model.Description,
            model.Color,
            Featured: false,
            model.Attribute,
            model.ExpectedDate is DateTime expectedDate ? DateOnly.FromDateTime(expectedDate) : null,
            model.Archived,
            DailyProjectStatus.Planned,
            ProgressPercentage: 0m,
            Todos: []));
    }

    // ---------------------------------------------------------------------------------------------
    // Delete — local removal, same RemovingItemId animation handshake as production.
    // ---------------------------------------------------------------------------------------------

    public Task DeleteCurrentHabitAsync() =>
        DeleteCurrentEditorItemAsync(DailyActivityType.Habit, localizer["HabitDeletedMessage"]);

    public Task DeleteCurrentTaskAsync() =>
        DeleteCurrentEditorItemAsync(DailyActivityType.Task, localizer["TaskDeletedMessage"]);

    public Task DeleteCurrentTodoAsync() =>
        DeleteCurrentEditorItemAsync(DailyActivityType.Todo, localizer["TodoDeletedMessage"]);

    public Task DeleteCurrentProjectAsync() =>
        DeleteCurrentEditorItemAsync(DailyActivityType.Project, localizer["ProjectDeletedMessage"]);

    private async Task DeleteCurrentEditorItemAsync(DailyActivityType expectedType, string successMessage)
    {
        if (Modals.EditingId is not Guid id || Modals.ActiveEditor != expectedType)
        {
            Modals.CloseEditor();
            return;
        }

        await ExecuteAsync(
            async () =>
            {
                RemovingItemId = id;
                Changed?.Invoke();
                await Task.Delay(RemovalAnimationMilliseconds, cancellation.Token);

                try
                {
                    await DelayAsync();
                    DeleteLocally(id, expectedType);
                    Modals.CloseEditor();
                    Changed?.Invoke();
                }
                finally
                {
                    // Only clear once the outcome is known — on success the item is already gone by
                    // the time this runs, so there is no flash.
                    RemovingItemId = null;
                }
            },
            successMessage,
            localizer["DeleteErrorMessage"]);
    }

    private void DeleteLocally(Guid id, DailyActivityType type)
    {
        switch (type)
        {
            case DailyActivityType.Habit:
                habits.RemoveAll(item => item.Id == id);
                break;
            case DailyActivityType.Task:
                tasks.RemoveAll(item => item.Id == id);
                break;
            case DailyActivityType.Todo:
                todos.RemoveAll(item => item.Id == id);
                break;
            case DailyActivityType.Project:
                projects.RemoveAll(item => item.Id == id);

                // Production's cascade is a Domain concern; locally, orphaned todos would otherwise
                // keep showing in the Todos column with no project to filter by.
                todos.RemoveAll(item => item.ProjectId == id);

                if (selectedProjectId == id)
                {
                    selectedProjectId = null;
                }

                if (OpenProjectId == id)
                {
                    OpenProjectId = null;
                }

                break;
        }
    }

    // ---------------------------------------------------------------------------------------------
    // Habit scoring / completion toggles.
    // ---------------------------------------------------------------------------------------------

    public Task RegisterPositiveAsync(Guid id) =>
        ExecuteAsync(async () =>
        {
            await DelayAsync();

            var index = IndexOf(habits, item => item.Id, id);
            if (index < 0)
            {
                return;
            }

            habits[index] = habits[index] with { PositiveCount = habits[index].PositiveCount + 1 };
            Changed?.Invoke();

            // A positive registration is always an experience-earning transition.
            AwardExperience(DailyExperienceSource.Habit);
        });

    public Task RegisterNegativeAsync(Guid id) =>
        ExecuteAsync(async () =>
        {
            await DelayAsync();

            var index = IndexOf(habits, item => item.Id, id);
            if (index < 0)
            {
                return;
            }

            // Production awards no experience for a negative registration — RegisterNegativeAsync
            // deliberately does not go through the experience path there either.
            habits[index] = habits[index] with { NegativeCount = habits[index].NegativeCount + 1 };
            Changed?.Invoke();
        });

    public Task ToggleTaskAsync(Guid id) =>
        ExecuteAsync(async () =>
        {
            await DelayAsync();

            var index = IndexOf(tasks, item => item.Id, id);
            if (index < 0)
            {
                return;
            }

            var completed = !tasks[index].Completed;
            tasks[index] = tasks[index] with { Completed = completed };
            Changed?.Invoke();

            // Mirrors production's `gainedExperience <= 0` guard: un-completing something must never
            // announce a gain.
            if (completed)
            {
                AwardExperience(DailyExperienceSource.Task);
            }
        });

    public Task ToggleTodoAsync(Guid id) =>
        ExecuteAsync(async () =>
        {
            await DelayAsync();

            var index = IndexOf(todos, item => item.Id, id);
            if (index < 0)
            {
                return;
            }

            var completed = !todos[index].Completed;

            // NOTE: the owning project's ProgressPercentage is deliberately NOT recalculated here.
            // It is a pre-resolved display value the scenario supplied (Issue #374 item 5) — deriving
            // it from the local todo counts would be exactly the aggregation the Lab must not
            // reproduce.
            todos[index] = todos[index] with { Completed = completed };
            Changed?.Invoke();

            if (completed)
            {
                AwardExperience(DailyExperienceSource.Todo);
            }
        });

    // ---------------------------------------------------------------------------------------------
    // Reorder — real SortableOrder.Move, applied to the local list instead of posted to a service.
    // ---------------------------------------------------------------------------------------------

    public Task ReorderHabitsAsync(SortableReorderEvent reorder) =>
        ReorderAsync(FilteredHabits.Select(item => item.Id).ToList(), reorder,
            order => ApplyOrder(habits, order, item => item.Id));

    public Task ReorderTasksAsync(SortableReorderEvent reorder) =>
        ReorderAsync(FilteredTasks.Select(item => item.Id).ToList(), reorder,
            order => ApplyOrder(tasks, order, item => item.Id));

    public Task ReorderTodosAsync(SortableReorderEvent reorder) =>
        ReorderAsync(FilteredTodos.Select(item => item.Id).ToList(), reorder,
            order => ApplyOrder(todos, order, item => item.Id));

    public Task ReorderProjectsAsync(SortableReorderEvent reorder) =>
        ReorderAsync(FilteredProjects.Select(item => item.Id).ToList(), reorder,
            order => ApplyOrder(projects, order, item => item.Id));

    private Task ReorderAsync(
        IReadOnlyList<Guid> currentOrder,
        SortableReorderEvent reorder,
        Action<IReadOnlyList<Guid>> applyOrder)
    {
        ArgumentNullException.ThrowIfNull(reorder);

        if (!Guid.TryParse(reorder.ItemId, out var itemId)
            || !Guid.TryParse(reorder.TargetItemId, out var targetItemId))
        {
            return Task.CompletedTask;
        }

        // The real, already-extracted presentation helper — unchanged from production's use of it.
        var reorderedIds = SortableOrder.Move(currentOrder, itemId, targetItemId, reorder.PlaceAfter);
        if (reorderedIds.SequenceEqual(currentOrder))
        {
            return Task.CompletedTask;
        }

        return ExecuteAsync(
            async () =>
            {
                await DelayAsync();
                applyOrder(reorderedIds);
                Changed?.Invoke();
            },
            errorMessage: localizer["ReorderErrorMessage"]);
    }

    /// <summary>
    /// Applies an order computed over a FILTERED subset back onto the full backing list: the
    /// positions the affected ids currently occupy are refilled in the new order, leaving every
    /// filtered-out item exactly where it was. This is what production's server-side reorder produces
    /// once the next load comes back, reproduced locally.
    /// </summary>
    private static void ApplyOrder<T>(List<T> items, IReadOnlyList<Guid> newOrder, Func<T, Guid> idOf)
    {
        var affected = newOrder.ToHashSet();
        var positions = new List<int>();

        for (var index = 0; index < items.Count; index++)
        {
            if (affected.Contains(idOf(items[index])))
            {
                positions.Add(index);
            }
        }

        var byId = items.Where(item => affected.Contains(idOf(item))).ToDictionary(idOf);

        for (var index = 0; index < positions.Count && index < newOrder.Count; index++)
        {
            if (byId.TryGetValue(newOrder[index], out var item))
            {
                items[positions[index]] = item;
            }
        }
    }

    // ---------------------------------------------------------------------------------------------
    // Formatting — ported verbatim, retyped to the Lab-local enums.
    // ---------------------------------------------------------------------------------------------

    public string FormatRepeat(DailyTaskRepeat repeat) => repeat switch
    {
        DailyTaskRepeat.None => localizer["NoRepeatLabel"],
        DailyTaskRepeat.Daily => localizer["TaskRepeatDaily"],
        DailyTaskRepeat.Weekly => localizer["TaskRepeatWeekly"],
        DailyTaskRepeat.Monthly => localizer["TaskRepeatMonthly"],
        _ => repeat.ToString()
    };

    public string FormatDueDate(DateOnly? date) =>
        date?.ToString("d") ?? localizer["NoDueDateLabel"];

    public string FormatProjectStatus(DailyProjectStatus status) => status switch
    {
        DailyProjectStatus.Planned => localizer["ProjectStatusPlanned"],
        DailyProjectStatus.InProgress => localizer["ProjectInProgressLabel"],
        DailyProjectStatus.Completed => localizer["ProjectStatusCompleted"],
        _ => status.ToString()
    };

    private IEnumerable<T> Filter<T>(
        IEnumerable<T> items,
        Func<T, string> title,
        Func<T, string> description) =>
        items.Where(item => MatchesSearch(title(item), description(item)));

    private bool MatchesSearch(string title, string description) =>
        string.IsNullOrWhiteSpace(search)
            || title.Contains(search, StringComparison.OrdinalIgnoreCase)
            || description.Contains(search, StringComparison.OrdinalIgnoreCase);

    // ---------------------------------------------------------------------------------------------
    // Experience feedback (FE33-096) — fixed constant, no diffing, no curve.
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    /// Replaces production's <c>ExecuteExperienceOperationAsync</c>/<c>ShowExperienceGain</c> pair,
    /// which captured the profile's <c>TotalExperience</c> before the mutation, reloaded from the
    /// backend, and diffed the two. The Lab has nothing to diff against, so a positive transition
    /// simply adds the scenario's fixed <c>XpGainPerAction</c> to the running display total and
    /// announces exactly that amount. The <c>LatestExperienceGain</c>/<c>ExperienceFeedbackVersion</c>
    /// pair and the 750 ms clear are ported unchanged.
    /// </summary>
    private void AwardExperience(DailyExperienceSource source)
    {
        AddExperienceForPositiveAction();

        if (xpGainPerAction <= 0)
        {
            return;
        }

        LatestExperienceGain = xpGainPerAction;
        ExperienceFeedbackVersion++;
        Changed?.Invoke();
        _ = ClearExperienceFeedbackAsync(ExperienceFeedbackVersion);

        MaybeRaiseMockLevelUp(source);
    }

    private void AddExperienceForPositiveAction()
    {
        positiveExperienceActionCount++;
        totalExperience += xpGainPerAction;
    }

    private async Task ClearExperienceFeedbackAsync(long feedbackVersion)
    {
        try
        {
            await Task.Delay(ExperienceFeedbackMilliseconds, cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (ExperienceFeedbackVersion != feedbackVersion)
        {
            return;
        }

        LatestExperienceGain = 0;
        Changed?.Invoke();
    }

    /// <summary>
    /// The MOCK level-up feed (FE33-096). See <see cref="LevelUpEveryNthPositiveAction"/> for why the
    /// trigger is "every 3rd positive action". The payload is synthetic throughout: the level simply
    /// steps by one — no XP threshold is consulted and no leveling curve is reproduced — and the
    /// amount shown is the same fixed <c>XpGainPerAction</c> the gain announcement used.
    /// </summary>
    private void MaybeRaiseMockLevelUp(DailyExperienceSource source)
    {
        if (positiveExperienceActionCount % LevelUpEveryNthPositiveAction != 0)
        {
            return;
        }

        var previousLevel = currentLevel;
        currentLevel = previousLevel + 1;

        feedbackStore.Add(new BeeDayFeedback(
            EventId: Guid.NewGuid(),
            ExperienceEntryId: Guid.NewGuid(),
            PreviousLevel: previousLevel,
            NewLevel: currentLevel,
            LevelsGained: 1,
            ExperienceAmount: xpGainPerAction,
            ExperienceSource: source,
            // No wall clock is available to the Lab's deterministic scenario layer, but this value is
            // never rendered — BeeDayFeedbackModal shows levels and the XP amount only.
            OccurredAtUtc: default));

        Changed?.Invoke();
    }

    // ---------------------------------------------------------------------------------------------
    // Shared plumbing.
    // ---------------------------------------------------------------------------------------------

    private Task DelayAsync() => Task.Delay(MutationDelayMilliseconds, cancellation.Token);

    private static int IndexOf<T>(List<T> items, Func<T, Guid> idOf, Guid? id) =>
        id is Guid value ? items.FindIndex(item => idOf(item) == value) : -1;

    private async Task SaveEditorAsync(Action mutate, string successMessage)
    {
        await ExecuteAsync(
            async () =>
            {
                await DelayAsync();
                mutate();
                Modals.CloseEditor();
                Changed?.Invoke();
            },
            successMessage,
            localizer["SaveErrorMessage"]);
    }

    private async Task ExecuteAsync(
        Func<Task> operation,
        string? successMessage = null,
        string? errorMessage = null)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;

        try
        {
            await operation();

            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                toastService.ShowSuccess(successMessage);
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            // The circuit is tearing down (Dispose already cancelled this token) — nothing left to
            // show a toast to.
        }
        catch
        {
            toastService.ShowError(errorMessage ?? localizer["GenericOperationErrorMessage"]);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
