using BeeDayLab.Web.Components.Behaviors.DragDrop;
using BeeDayLab.Web.Components.DesignSystem.Feedback;
using BeeDayLab.Web.Components.Pages.Daily;
using BeeDayLab.Web.Components.Pages.Daily.Experience.Feedback;
using BeeDayLab.Web.Components.Pages.Daily.Habits.Models;
using BeeDayLab.Web.Components.Pages.Daily.State;
using BeeDayLab.Web.Components.Pages.Daily.Tasks.Models;
using BeeDayLab.Web.Scenarios;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Sprint 33.13 (FE33-090) tests for <see cref="LabDashboardState"/> — the Lab port of production's
/// <c>DashboardState</c>. Three things are proven here:
///
/// <list type="number">
/// <item>the client-side search/filter/counting surface, ported verbatim, still behaves exactly as
/// production's does across all four collections (Issue #374 item 4);</item>
/// <item>every editor mutation is inert and local — items are added/updated/removed/toggled in the
/// in-memory working copy only, and a project's pre-resolved <c>ProgressPercentage</c> is never
/// recalculated behind the caller's back (Issue #374 items 3 and 5);</item>
/// <item>the experience-gain announcement and the mock level-up feed fire on exactly the transitions
/// production fires them on — and, critically, NOT when un-completing something.</item>
/// </list>
/// </summary>
public sealed class DashboardStateTests
{
    // Facts about the Populated scenario, asserted once here so the rest of the file can rely on
    // them. If DailyDashboardScenarioProvider's sample data changes, this test is the first to say so.
    [Fact]
    public void PopulatedScenarioHasTheExpectedShape()
    {
        var state = CreateState(ScenarioState.Populated);

        Assert.Equal(7, state.Habits.Count);
        Assert.Equal(6, state.Tasks.Count);
        Assert.Equal(3, state.Projects.Count);
        Assert.Equal(25, state.TotalItems);
        Assert.Equal(8, state.CompletedItems);
        Assert.Equal(17, state.ActiveItems);
        Assert.False(state.IsLoading);
        Assert.False(state.IsUnavailable);
        Assert.True(state.HasProfile);
    }

    [Fact]
    public void PopulatedHabitsCoverAllSevenVisualStateBands()
    {
        var state = CreateState(ScenarioState.Populated);

        var bands = state.Habits
            .Select(habit => BeeDayLab.Web.Components.Pages.Daily.Habits.HabitVisualState
                .GetModifier(habit.PositiveCount - habit.NegativeCount))
            .Distinct()
            .ToList();

        Assert.Equal(7, bands.Count);
    }

    [Fact]
    public void EmptyScenarioHasNoItemsButStillRendersAProfile()
    {
        var state = CreateState(ScenarioState.Empty);

        Assert.Empty(state.Habits);
        Assert.Empty(state.Tasks);
        Assert.Empty(state.Projects);
        Assert.Equal(0, state.TotalItems);

        // The page gates its whole render on HasProfile; an Empty scenario must still preview the
        // four empty columns rather than a redirect.
        Assert.True(state.HasProfile);
        Assert.False(state.IsUnavailable);
    }

    [Fact]
    public void LargeContentScenarioExceedsTheSortableVirtualizationThresholdInEverySortableCollection()
    {
        const int VirtualizationThreshold = 30;
        var state = CreateState(ScenarioState.LargeContent);

        Assert.True(state.FilteredHabits.Count() >= VirtualizationThreshold);
        Assert.True(state.FilteredTasks.Count(task => !task.Completed) >= VirtualizationThreshold);
        Assert.True(state.FilteredTodos.Count(todo => !todo.Completed) >= VirtualizationThreshold);
        Assert.True(state.FilteredProjects.Count(project => !project.Completed) >= VirtualizationThreshold);
    }

    [Fact]
    public void ErrorScenarioShowsAnErrorToastAndMarksTheBoardUnavailable()
    {
        var toast = new ToastService();
        var state = CreateState(ScenarioState.Error, toast);

        Assert.True(state.IsUnavailable);
        Assert.False(state.IsLoading);
        Assert.Empty(state.Habits);
        Assert.NotEmpty(toast.Messages);
        Assert.Equal(ToastVariant.Error, toast.Messages[^1].Variant);
    }

    [Fact]
    public void LoadingScenarioNeverLeavesTheLoadingState()
    {
        // The caller renders BeeDayDashboardSkeleton for as long as IsLoading is true; the provider is
        // deliberately never asked for data in this state.
        var state = CreateState(ScenarioState.Loading);

        Assert.True(state.IsLoading);
        Assert.False(state.HasProfile);
    }

    // -----------------------------------------------------------------------------------------
    // Search / filter / counting — the surface Issue #374 item 4 requires be preserved faithfully.
    // -----------------------------------------------------------------------------------------

    [Fact]
    public void SearchMatchesTitleAndDescriptionCaseInsensitivelyAcrossAllFourCollections()
    {
        var state = CreateState(ScenarioState.Populated);

        // Title match, upper-cased: only the "Pick the paint colours" to-do carries this word.
        state.Search = "COLOURS";

        Assert.Empty(state.FilteredHabits);
        Assert.Empty(state.FilteredTasks);
        Assert.Single(state.FilteredTodos);
        Assert.Empty(state.FilteredProjects);
        Assert.Equal(1, state.FilteredItems);

        // A description-only match still counts (production's MatchesSearch checks both fields).
        state.Search = "external drive";
        Assert.Single(state.FilteredTasks);

        state.Search = "rewire";
        Assert.Single(state.FilteredProjects);

        // And a substring anywhere in either field matches, not just a whole word: "paint" is inside
        // the to-do's title AND inside the Apartment project's "Repaint, rewire..." description.
        state.Search = "paint";
        Assert.Single(state.FilteredTodos);
        Assert.Single(state.FilteredProjects);
        Assert.Equal(2, state.FilteredItems);
    }

    [Fact]
    public void FilteredToZeroFlagsDistinguishAnUnmatchedFilterFromAGenuinelyEmptyCollection()
    {
        var state = CreateState(ScenarioState.Populated);

        // No search: nothing is "filtered to zero", even though data exists.
        Assert.False(state.HabitsFilteredToZero);
        Assert.False(state.ActiveTasksFilteredToZero);
        Assert.False(state.ActiveTodosFilteredToZero);
        Assert.False(state.ActiveProjectsFilteredToZero);

        state.Search = "zzz-no-such-item";

        Assert.True(state.HabitsFilteredToZero);
        Assert.True(state.ActiveTasksFilteredToZero);
        Assert.True(state.ActiveTodosFilteredToZero);
        Assert.True(state.ActiveProjectsFilteredToZero);

        // On an Empty scenario the same unmatched search must NOT claim "filtered to zero" — there is
        // genuinely nothing there, which is a different narrative.
        var empty = CreateState(ScenarioState.Empty);
        empty.Search = "zzz-no-such-item";

        Assert.False(empty.HabitsFilteredToZero);
        Assert.False(empty.ActiveTasksFilteredToZero);
        Assert.False(empty.ActiveTodosFilteredToZero);
        Assert.False(empty.ActiveProjectsFilteredToZero);
    }

    [Fact]
    public void SelectProjectContextNarrowsOnlyTheTodosColumn()
    {
        var state = CreateState(ScenarioState.Populated);
        var project = state.Projects.First();
        var todosInProject = state.FilteredTodos.Count(todo => todo.ProjectId == project.Id);

        state.SelectProjectContext(project.Id);

        Assert.Equal(project.Id, state.SelectedProjectId);
        Assert.Equal(todosInProject, state.FilteredTodos.Count());
        Assert.All(state.FilteredTodos, todo => Assert.Equal(project.Id, todo.ProjectId));

        // Every other column is untouched by the project context.
        Assert.Equal(state.Habits.Count, state.FilteredHabits.Count());
        Assert.Equal(state.Tasks.Count, state.FilteredTasks.Count());
        Assert.Equal(state.Projects.Count, state.FilteredProjects.Count());
    }

    [Fact]
    public void SelectProjectContextIgnoresAnUnknownProjectId()
    {
        var state = CreateState(ScenarioState.Populated);

        state.SelectProjectContext(Guid.NewGuid());

        Assert.Null(state.SelectedProjectId);
    }

    [Fact]
    public void ProjectContextAloneCanTriggerTheTodosFilteredToZeroNarrative()
    {
        var state = CreateState(ScenarioState.Populated);

        // The completed marathon project has no ACTIVE todos left.
        var completedProject = state.Projects.First(project => project.Completed);
        state.SelectProjectContext(completedProject.Id);

        Assert.True(state.ActiveTodosFilteredToZero);

        // ...and that narrative is reachable without any search text at all, unlike the other three.
        Assert.Equal(string.Empty, state.Search);
    }

    [Fact]
    public void ClearFiltersResetsBothTheSearchTextAndTheProjectContext()
    {
        var state = CreateState(ScenarioState.Populated);
        state.Search = "paint";
        state.SelectProjectContext(state.Projects.First().Id);

        state.ClearFilters();

        Assert.Equal(string.Empty, state.Search);
        Assert.Null(state.SelectedProjectId);
        Assert.Equal(state.TotalItems, state.FilteredItems);
    }

    [Fact]
    public void SearchSetterCoercesNullToEmptyRatherThanThrowing()
    {
        var state = CreateState(ScenarioState.Populated);

        state.Search = null!;

        Assert.Equal(string.Empty, state.Search);
    }

    // -----------------------------------------------------------------------------------------
    // Local, inert mutation.
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task SaveHabitCreatesLocallyWhenNoItemIsBeingEdited()
    {
        var state = CreateState(ScenarioState.Populated);
        var before = state.Habits.Count;
        state.OpenCreate(DailyActivityType.Habit);

        await state.SaveHabitAsync(new HabitEditorModel { Title = "Stretch", Description = "Five minutes." });

        Assert.Equal(before + 1, state.Habits.Count);
        Assert.Contains(state.Habits, habit => habit.Title == "Stretch");

        // A newly created habit starts at a zero balance — no counters are invented for it.
        var created = state.Habits.First(habit => habit.Title == "Stretch");
        Assert.Equal(0, created.PositiveCount);
        Assert.Equal(0, created.NegativeCount);

        // The editor closes on success, exactly as production's SaveEditorAsync does.
        Assert.Null(state.Modals.ActiveEditor);
    }

    [Fact]
    public async Task SaveHabitUpdatesInPlaceWhenAnItemIsBeingEdited()
    {
        var state = CreateState(ScenarioState.Populated);
        var target = state.Habits[0];
        var before = state.Habits.Count;
        state.OpenHabitEditor(target);

        var model = state.Modals.HabitForm;
        model.Title = "Renamed habit";
        await state.SaveHabitAsync(model);

        Assert.Equal(before, state.Habits.Count);
        var updated = state.Habits.Single(habit => habit.Id == target.Id);
        Assert.Equal("Renamed habit", updated.Title);

        // Scenario-resolved counters survive an edit untouched.
        Assert.Equal(target.PositiveCount, updated.PositiveCount);
        Assert.Equal(target.NegativeCount, updated.NegativeCount);
    }

    [Fact]
    public async Task DeleteCurrentTaskRemovesTheItemLocallyAndClearsTheRemovalMarker()
    {
        var state = CreateState(ScenarioState.Populated);
        var target = state.Tasks[0];
        state.OpenTaskEditor(target);

        await state.DeleteCurrentTaskAsync();

        Assert.DoesNotContain(state.Tasks, task => task.Id == target.Id);
        Assert.Null(state.RemovingItemId);
        Assert.Null(state.Modals.ActiveEditor);
    }

    [Fact]
    public async Task DeletingAProjectAlsoDropsItsTodosAndAnyFilterPointingAtIt()
    {
        var state = CreateState(ScenarioState.Populated);
        var target = state.Projects.First(project => !project.Completed);
        state.SelectProjectContext(target.Id);
        state.OpenProjectWorkspace(target);
        state.OpenProjectEditor(target);

        await state.DeleteCurrentProjectAsync();

        Assert.DoesNotContain(state.Projects, project => project.Id == target.Id);
        Assert.DoesNotContain(state.FilteredTodos, todo => todo.ProjectId == target.Id);
        Assert.Null(state.SelectedProjectId);
        Assert.Null(state.OpenProjectId);
    }

    [Fact]
    public async Task DeleteIsIgnoredWhenTheOpenEditorIsForADifferentActivityType()
    {
        var state = CreateState(ScenarioState.Populated);
        var before = state.Habits.Count;
        state.OpenTaskEditor(state.Tasks[0]);

        await state.DeleteCurrentHabitAsync();

        Assert.Equal(before, state.Habits.Count);
        Assert.Null(state.Modals.ActiveEditor);
    }

    [Fact]
    public async Task ToggleTaskFlipsCompletionBothWays()
    {
        var state = CreateState(ScenarioState.Populated);
        var target = state.Tasks.First(task => !task.Completed);

        await state.ToggleTaskAsync(target.Id);
        Assert.True(state.Tasks.Single(task => task.Id == target.Id).Completed);

        await state.ToggleTaskAsync(target.Id);
        Assert.False(state.Tasks.Single(task => task.Id == target.Id).Completed);
    }

    [Fact]
    public async Task RegisterPositiveAndNegativeOnlyMoveTheirOwnCounter()
    {
        var state = CreateState(ScenarioState.Populated);
        var target = state.Habits[0];

        await state.RegisterPositiveAsync(target.Id);
        var afterPositive = state.Habits.Single(habit => habit.Id == target.Id);
        Assert.Equal(target.PositiveCount + 1, afterPositive.PositiveCount);
        Assert.Equal(target.NegativeCount, afterPositive.NegativeCount);

        await state.RegisterNegativeAsync(target.Id);
        var afterNegative = state.Habits.Single(habit => habit.Id == target.Id);
        Assert.Equal(target.PositiveCount + 1, afterNegative.PositiveCount);
        Assert.Equal(target.NegativeCount + 1, afterNegative.NegativeCount);
    }

    [Fact]
    public async Task TogglingATodoDoesNotRecalculateItsProjectsProgressPercentage()
    {
        // Issue #374 item 5: ProgressPercentage is a pre-resolved display value from the scenario.
        // Deriving it locally from the todo counts is exactly the aggregation the Lab must not do.
        var state = CreateState(ScenarioState.Populated);
        var project = state.Projects.First(candidate => !candidate.Completed);
        var progressBefore = project.ProgressPercentage;
        var todo = state.FilteredTodos.First(candidate => candidate.ProjectId == project.Id && !candidate.Completed);

        await state.ToggleTodoAsync(todo.Id);

        Assert.True(state.FilteredTodos.Single(candidate => candidate.Id == todo.Id).Completed);
        Assert.Equal(progressBefore, state.Projects.Single(candidate => candidate.Id == project.Id).ProgressPercentage);
    }

    [Fact]
    public async Task ReorderMovesTheItemLocallyUsingTheRealSortableOrderHelper()
    {
        var state = CreateState(ScenarioState.Populated);
        var ids = state.FilteredHabits.Select(habit => habit.Id).ToList();
        var expected = SortableOrder.Move(ids, ids[0], ids[2], placeAfter: true);

        await state.ReorderHabitsAsync(new SortableReorderEvent(
            ids[0].ToString(), ids[2].ToString(), PlaceAfter: true));

        Assert.Equal(expected, state.Habits.Select(habit => habit.Id).ToList());
    }

    [Fact]
    public async Task ReorderIsANoOpForUnparsableIdsOrAnUnchangedOrder()
    {
        var state = CreateState(ScenarioState.Populated);
        var before = state.Habits.Select(habit => habit.Id).ToList();

        await state.ReorderHabitsAsync(new SortableReorderEvent("not-a-guid", "also-not", PlaceAfter: false));
        Assert.Equal(before, state.Habits.Select(habit => habit.Id).ToList());

        // Moving an item onto itself produces the same order, so nothing is applied.
        await state.ReorderHabitsAsync(new SortableReorderEvent(
            before[0].ToString(), before[0].ToString(), PlaceAfter: false));
        Assert.Equal(before, state.Habits.Select(habit => habit.Id).ToList());
    }

    [Fact]
    public async Task OpenProjectReNestsItsTodosForTheWorkspacePanel()
    {
        var state = CreateState(ScenarioState.Populated);
        var project = state.Projects.First(candidate => !candidate.Completed);

        state.OpenProjectWorkspace(project);

        Assert.NotNull(state.OpenProject);
        Assert.NotEmpty(state.OpenProject!.Todos);
        Assert.All(state.OpenProject.Todos, todo => Assert.Equal(project.Id, todo.ProjectId));

        // ...and it reflects local mutations immediately, since it is re-derived on every read.
        var todo = state.OpenProject.Todos.First(candidate => !candidate.Completed);
        await state.ToggleTodoAsync(todo.Id);
        Assert.True(state.OpenProject!.Todos.Single(candidate => candidate.Id == todo.Id).Completed);

        state.CloseProjectWorkspace();
        Assert.Null(state.OpenProject);
    }

    [Fact]
    public async Task SaveTaskShowsASuccessToastAndNothingLeaksIntoAnyOtherCollection()
    {
        var toast = new ToastService();
        var state = CreateState(ScenarioState.Populated, toast);
        var habitsBefore = state.Habits.Count;
        var projectsBefore = state.Projects.Count;
        state.OpenCreate(DailyActivityType.Task);

        await state.SaveTaskAsync(new TaskEditorModel { Title = "New task", Repeat = DailyTaskRepeat.Weekly });

        Assert.NotEmpty(toast.Messages);
        Assert.Equal(ToastVariant.Success, toast.Messages[^1].Variant);
        Assert.Equal(habitsBefore, state.Habits.Count);
        Assert.Equal(projectsBefore, state.Projects.Count);
    }

    // -----------------------------------------------------------------------------------------
    // Experience feedback + mock level-up.
    // -----------------------------------------------------------------------------------------

    [Fact]
    public async Task CompletingAnItemAddsTheFixedScenarioXpAndAnnouncesExactlyThatAmount()
    {
        var state = CreateState(ScenarioState.Populated);
        var totalBefore = state.Profile.TotalExperience;
        var versionBefore = state.ExperienceFeedbackVersion;
        var target = state.Tasks.First(task => !task.Completed);

        await state.ToggleTaskAsync(target.Id);

        var gain = state.LatestExperienceGain;
        Assert.Equal(10, gain);
        Assert.Equal(totalBefore + gain, state.Profile.TotalExperience);
        Assert.Equal(versionBefore + 1, state.ExperienceFeedbackVersion);
    }

    [Fact]
    public async Task TheExperienceGainAnnouncementClearsItselfAfterItsDisplayWindow()
    {
        var state = CreateState(ScenarioState.Populated);
        var target = state.Tasks.First(task => !task.Completed);

        await state.ToggleTaskAsync(target.Id);
        Assert.Equal(10, state.LatestExperienceGain);

        // Production's window is 750 ms; wait past it with margin.
        await Task.Delay(1_200, TestContext.Current.CancellationToken);

        Assert.Equal(0, state.LatestExperienceGain);

        // The running total is NOT rolled back — only the transient announcement clears.
        Assert.True(state.Profile.TotalExperience > 0);
    }

    [Fact]
    public async Task UnCompletingSomethingNeverAnnouncesAGain()
    {
        // Mirrors production's `gainedExperience <= 0` guard.
        var state = CreateState(ScenarioState.Populated);
        var target = state.Tasks.First(task => task.Completed);
        var totalBefore = state.Profile.TotalExperience;
        var versionBefore = state.ExperienceFeedbackVersion;

        await state.ToggleTaskAsync(target.Id);

        Assert.False(state.Tasks.Single(task => task.Id == target.Id).Completed);
        Assert.Equal(0, state.LatestExperienceGain);
        Assert.Equal(totalBefore, state.Profile.TotalExperience);
        Assert.Equal(versionBefore, state.ExperienceFeedbackVersion);
    }

    [Fact]
    public async Task RegisteringAHabitNegativeAwardsNoExperience()
    {
        var state = CreateState(ScenarioState.Populated);
        var totalBefore = state.Profile.TotalExperience;

        await state.RegisterNegativeAsync(state.Habits[0].Id);

        Assert.Equal(totalBefore, state.Profile.TotalExperience);
        Assert.Equal(0, state.LatestExperienceGain);
    }

    [Fact]
    public async Task EveryThirdPositiveActionRaisesTheMockLevelUpFeedback()
    {
        var store = new BeeDayFeedbackStore();
        var state = CreateState(ScenarioState.Populated, feedbackStore: store);
        var levelBefore = state.Profile.CurrentLevel;
        var active = state.Tasks.Where(task => !task.Completed).Select(task => task.Id).Take(3).ToList();

        await state.ToggleTaskAsync(active[0]);
        Assert.Null(store.Current);

        await state.ToggleTaskAsync(active[1]);
        Assert.Null(store.Current);

        await state.ToggleTaskAsync(active[2]);

        Assert.NotNull(store.Current);
        Assert.Equal(levelBefore, store.Current!.PreviousLevel);
        Assert.Equal(levelBefore + 1, store.Current.NewLevel);
        Assert.Equal(1, store.Current.LevelsGained);
        Assert.Equal(10, store.Current.ExperienceAmount);
        Assert.Equal(DailyExperienceSource.Task, store.Current.ExperienceSource);

        // The displayed level moves with the modal, so the ExperienceBar and the modal agree.
        Assert.Equal(levelBefore + 1, state.Profile.CurrentLevel);
        Assert.Single(store.History);
    }

    [Fact]
    public async Task TheMockLevelUpAttributesItsSourceToTheActivityThatTriggeredIt()
    {
        var store = new BeeDayFeedbackStore();
        var state = CreateState(ScenarioState.Populated, feedbackStore: store);
        var habitId = state.Habits[0].Id;

        await state.RegisterPositiveAsync(habitId);
        await state.RegisterPositiveAsync(habitId);
        await state.RegisterPositiveAsync(habitId);

        Assert.NotNull(store.Current);
        Assert.Equal(DailyExperienceSource.Habit, store.Current!.ExperienceSource);
    }

    [Fact]
    public async Task ConsumingTheLevelUpFeedbackClearsTheModalButKeepsTheHistory()
    {
        var store = new BeeDayFeedbackStore();
        var state = CreateState(ScenarioState.Populated, feedbackStore: store);
        var active = state.Tasks.Where(task => !task.Completed).Select(task => task.Id).Take(3).ToList();

        foreach (var id in active)
        {
            await state.ToggleTaskAsync(id);
        }

        Assert.NotNull(store.Current);

        store.Consume();

        Assert.Null(store.Current);
        Assert.Single(store.History);
    }

    // -----------------------------------------------------------------------------------------

    private static LabDashboardState CreateState(
        ScenarioState scenarioState,
        ToastService? toastService = null,
        BeeDayFeedbackStore? feedbackStore = null)
    {
        var state = new LabDashboardState(
            toastService ?? new ToastService(),
            new ScenarioSelection { State = scenarioState },
            new DailyDashboardScenarioProvider(),
            feedbackStore ?? new BeeDayFeedbackStore(),
            BuildLocalizer());

        // InitializeAsync completes synchronously in the Lab (no I/O), so this is safe to await here
        // and leaves every test looking at a fully seeded board.
        state.InitializeAsync().GetAwaiter().GetResult();
        return state;
    }

    private static IStringLocalizer<DashboardResources> BuildLocalizer()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None));
        services.AddLocalization();

        return services.BuildServiceProvider().GetRequiredService<IStringLocalizer<DashboardResources>>();
    }
}
