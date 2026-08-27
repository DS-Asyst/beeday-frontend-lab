using BeeDayLab.Web.Components.Pages.Daily.Habits.Components;
using BeeDayLab.Web.Components.Pages.Daily.Habits.Models;
using BeeDayLab.Web.Components.Pages.Daily.Projects.Components;
using BeeDayLab.Web.Components.Pages.Daily.Projects.Models;
using BeeDayLab.Web.Components.Pages.Daily.Tasks.Components;
using BeeDayLab.Web.Components.Pages.Daily.Tasks.Models;
using BeeDayLab.Web.Components.Pages.Daily.Todos.Components;
using BeeDayLab.Web.Components.Pages.Daily.Todos.Models;
using BeeDayLab.Web.Scenarios;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Sprint 33.13 (FE33-091..095) tests for the four activity editor dialogs and the project workspace
/// panel. All five are presentation-only: they compose <c>EditorModalShell</c>/<c>BeeDayConfirmDialog</c>
/// (already in the Lab since Sprint 33.8) and raise <c>EventCallback</c>s — the caller decides what a
/// save or delete actually does, which in the Lab is a local, inert mutation.
///
/// <para>These tests also pin the Lab-local enum retypings: each dialog must render one option per
/// member of the <c>Daily*</c> enum it lists, localized, with no Domain type anywhere in the chain.</para>
/// </summary>
public sealed class ActivityEditorModalTests
{
    // ------------------------------------------------------------------------------------------
    // Habit editor (FE33-091)
    // ------------------------------------------------------------------------------------------

    [Fact]
    public void HabitEditorRendersEveryDifficultyAndResetCounterOption()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext();

        var cut = context.Render<HabitEditorModal>(parameters => parameters
            .Add(p => p.Model, new HabitEditorModel { Title = "Morning water" }));

        Assert.Equal(
            Enum.GetValues<DailyHabitDifficulty>().Length,
            cut.FindAll("#habit-difficulty option").Count);

        Assert.Equal(
            Enum.GetValues<DailyHabitResetCounter>().Length,
            cut.FindAll("#habit-reset-counter option").Count);

        // Localized, not raw enum names.
        Assert.Contains("Easy", cut.Markup, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(24, "habit-editor--sky")]
    [InlineData(0, "habit-editor--white")]
    [InlineData(-18, "habit-editor--red-strong")]
    public void HabitEditorWearsTheBandOfItsScenarioSeededVisualBalance(int balance, string expectedClass)
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext();

        var cut = context.Render<HabitEditorModal>(parameters => parameters
            .Add(p => p.Model, new HabitEditorModel { Title = "Habit", VisualBalance = balance }));

        Assert.Contains(expectedClass, cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void HabitEditorDirectionTogglesCycleThroughTheThreeLabDirections()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext();
        var model = new HabitEditorModel { Title = "Habit", Direction = DailyHabitDirection.Both };

        var cut = context.Render<HabitEditorModal>(parameters => parameters.Add(p => p.Model, model));

        var buttons = cut.FindAll(".habit-editor__direction-button");
        Assert.Equal(2, buttons.Count);

        // From Both, pressing "+" leaves only Negative allowed; pressing it again returns to Both.
        buttons[0].Click();
        Assert.Equal(DailyHabitDirection.Negative, model.Direction);

        cut.FindAll(".habit-editor__direction-button")[0].Click();
        Assert.Equal(DailyHabitDirection.Both, model.Direction);

        cut.FindAll(".habit-editor__direction-button")[1].Click();
        Assert.Equal(DailyHabitDirection.Positive, model.Direction);
    }

    [Fact]
    public void HabitEditorShowsTheDeleteActionOnlyWhenEditing()
    {
        using var culture = new TestCultureScope();

        using (var creating = CreateContext())
        {
            var cut = creating.Render<HabitEditorModal>(parameters => parameters
                .Add(p => p.Model, new HabitEditorModel { Title = "Habit" })
                .Add(p => p.IsEditing, false));

            Assert.Empty(cut.FindAll(".editor-modal__delete-action"));
        }

        using (var editing = CreateContext())
        {
            var cut = editing.Render<HabitEditorModal>(parameters => parameters
                .Add(p => p.Model, new HabitEditorModel { Title = "Habit" })
                .Add(p => p.IsEditing, true));

            Assert.NotEmpty(cut.FindAll(".editor-modal__delete-action"));
        }
    }

    [Fact]
    public void HabitEditorRoutesDeleteThroughTheConfirmationDialog()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext();
        var deleted = false;

        var cut = context.Render<HabitEditorModal>(parameters => parameters
            .Add(p => p.Model, new HabitEditorModel { Title = "Habit" })
            .Add(p => p.IsEditing, true)
            .Add(p => p.OnDelete, EventCallback.Factory.Create(this, () => deleted = true)));

        cut.Find(".editor-modal__delete-action").Click();

        // The callback must NOT fire until the confirmation is accepted.
        Assert.False(deleted);

        cut.Find(".delete-confirmation__confirm-action").Click();

        Assert.True(deleted);
    }

    // ------------------------------------------------------------------------------------------
    // Task editor (FE33-092)
    // ------------------------------------------------------------------------------------------

    [Fact]
    public void TaskEditorRendersOneLocalizedOptionPerLabTaskRepeatMember()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext();

        var cut = context.Render<TaskEditorModal>(parameters => parameters
            .Add(p => p.Model, new TaskEditorModel { Title = "Stand-up" }));

        Assert.Equal(Enum.GetValues<DailyTaskRepeat>().Length, cut.FindAll("#task-repeat option").Count);
        Assert.Contains("Daily", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Weekly", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Monthly", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void TaskEditorRaisesOnSaveWithItsOwnModel()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext();
        var model = new TaskEditorModel { Title = "Stand-up" };
        TaskEditorModel? saved = null;

        var cut = context.Render<TaskEditorModal>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.OnSave, EventCallback.Factory.Create<TaskEditorModel>(this, value => saved = value)));

        cut.Find("form").Submit();

        Assert.Same(model, saved);
    }

    // ------------------------------------------------------------------------------------------
    // To-do editor (FE33-093)
    // ------------------------------------------------------------------------------------------

    [Fact]
    public void TodoEditorListsOnlyNonArchivedProjectsFromTheLabProjectSummary()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext();

        var active = CreateProject("Apartment refresh", archived: false);
        var archived = CreateProject("Old project", archived: true);

        var cut = context.Render<TodoEditorModal>(parameters => parameters
            .Add(p => p.Model, new TodoEditorModel { Title = "Pick paint" })
            .Add(p => p.Projects, new[] { active, archived }));

        var options = cut.FindAll("#todo-project option");

        // One placeholder ("Select a project") plus exactly the non-archived projects.
        Assert.Equal(2, options.Count);
        Assert.Contains("Apartment refresh", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Old project", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void TodoEditorRaisesOnSaveWithItsOwnModel()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext();
        var model = new TodoEditorModel { Title = "Pick paint", ProjectId = Guid.NewGuid() };
        TodoEditorModel? saved = null;

        var cut = context.Render<TodoEditorModal>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.OnSave, EventCallback.Factory.Create<TodoEditorModel>(this, value => saved = value)));

        cut.Find("form").Submit();

        Assert.Same(model, saved);
    }

    // ------------------------------------------------------------------------------------------
    // Project editor (FE33-094)
    // ------------------------------------------------------------------------------------------

    [Fact]
    public void ProjectEditorShowsTheOpenProjectSecondaryActionOnlyWhenEditing()
    {
        using var culture = new TestCultureScope();
        var opened = false;

        using (var creating = CreateContext())
        {
            var cut = creating.Render<ProjectEditorModal>(parameters => parameters
                .Add(p => p.Model, new ProjectEditorModel { Title = "Apartment refresh" })
                .Add(p => p.IsEditing, false));

            Assert.DoesNotContain("Open project", cut.Markup, StringComparison.OrdinalIgnoreCase);
        }

        using (var editing = CreateContext())
        {
            var cut = editing.Render<ProjectEditorModal>(parameters => parameters
                .Add(p => p.Model, new ProjectEditorModel { Title = "Apartment refresh" })
                .Add(p => p.IsEditing, true)
                .Add(p => p.OnOpenProject, EventCallback.Factory.Create(this, () => opened = true)));

            var secondary = cut.FindAll(".editor-modal__footer-actions button")
                .First(button => button.TextContent.Contains("Open", StringComparison.OrdinalIgnoreCase));
            secondary.Click();

            Assert.True(opened);
        }
    }

    [Fact]
    public void ProjectEditorDefaultsToTheFixedDesignSystemAccentColor()
    {
        // The colour field is not user-editable any more; the model just has to keep carrying the
        // fixed accent so the shape still matches production's.
        Assert.Equal("#8056C7", new ProjectEditorModel().Color);
        Assert.Equal(ProjectEditorModel.ProjectAccentColor, new ProjectEditorModel().Color);
    }

    // ------------------------------------------------------------------------------------------
    // Project workspace (FE33-095)
    // ------------------------------------------------------------------------------------------

    [Fact]
    public void ProjectWorkspaceRendersNothingWithoutAProject()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext();

        var cut = context.Render<ProjectWorkspace>(parameters => parameters.Add(p => p.Project, null));

        Assert.Empty(cut.FindAll(".project-workspace"));
    }

    [Fact]
    public void ProjectWorkspaceShowsItsNestedTodosAndPreResolvedProgress()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext();

        var projectId = Guid.NewGuid();
        var project = CreateProject("Apartment refresh", archived: false) with
        {
            Id = projectId,
            Status = DailyProjectStatus.InProgress,
            ProgressPercentage = 62.5m,
            Todos =
            [
                CreateTodo(projectId, "Pick the paint colours", completed: true),
                CreateTodo(projectId, "Book the electrician", completed: false)
            ]
        };

        var cut = context.Render<ProjectWorkspace>(parameters => parameters.Add(p => p.Project, project));

        var dialog = cut.Find(".project-workspace__panel");
        Assert.Equal("dialog", dialog.GetAttribute("role"));
        Assert.Equal("true", dialog.GetAttribute("aria-modal"));

        Assert.Equal(2, cut.FindAll(".project-workspace__todo").Count);

        // The "1 / 2" completed count is a display count over the todos it was handed; 62.5% is the
        // scenario's own pre-resolved figure and is rendered as-is, not derived from that count.
        Assert.Contains("1 / 2", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("62.5%", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("In Progress", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void ProjectWorkspaceRaisesToggleAndCloseCallbacks()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext();

        var projectId = Guid.NewGuid();
        var todo = CreateTodo(projectId, "Book the electrician", completed: false);
        var project = CreateProject("Apartment refresh", archived: false) with
        {
            Id = projectId,
            Todos = [todo]
        };

        DailyTodoSummary? toggled = null;
        var closed = false;

        var cut = context.Render<ProjectWorkspace>(parameters => parameters
            .Add(p => p.Project, project)
            .Add(p => p.OnToggleTodo, EventCallback.Factory.Create<DailyTodoSummary>(this, value => toggled = value))
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => closed = true)));

        cut.Find(".project-workspace__todo-toggle").Click();
        Assert.Equal(todo.Id, toggled?.Id);

        cut.Find(".project-workspace__close").Click();
        Assert.True(closed);
    }

    [Fact]
    public void ProjectWorkspaceShowsAnEmptyStateWhenTheProjectHasNoTodos()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext();

        var project = CreateProject("Empty project", archived: false);

        var cut = context.Render<ProjectWorkspace>(parameters => parameters.Add(p => p.Project, project));

        Assert.Empty(cut.FindAll(".project-workspace__todo"));
        Assert.NotEmpty(cut.FindAll(".project-workspace__empty"));
    }

    // ------------------------------------------------------------------------------------------

    private static DailyProjectSummary CreateProject(string name, bool archived) => new(
        Guid.NewGuid(),
        name,
        $"{name} description.",
        ProjectEditorModel.ProjectAccentColor,
        Featured: false,
        DailyActivityAttribute.Strength,
        ExpectedDate: null,
        archived,
        DailyProjectStatus.Planned,
        ProgressPercentage: 0m,
        Todos: []);

    private static DailyTodoSummary CreateTodo(Guid projectId, string title, bool completed) => new(
        Guid.NewGuid(),
        title,
        $"{title} description.",
        projectId,
        Featured: false,
        DueDate: null,
        DailyActivityAttribute.Dexterity,
        completed,
        CreatedAtUtc: default,
        UpdatedAtUtc: default);

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();

        var dialogFocus = context.JSInterop.SetupModule("./js/beeday-dialog-focus.js");
        dialogFocus.SetupVoid("deactivate", _ => true);
        dialogFocus.Setup<bool>("activate", _ => true).SetResult(true);
        dialogFocus.SetupVoid("focusFirstInvalid", _ => true);

        context.Services.AddLocalization();

        return context;
    }
}
