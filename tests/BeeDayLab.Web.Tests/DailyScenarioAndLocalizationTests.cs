using System.Globalization;
using BeeDayLab.Web.Components.Pages.Daily;
using BeeDayLab.Web.Components.Pages.Daily.Experience;
using BeeDayLab.Web.Components.Pages.Daily.Experience.Models;
using BeeDayLab.Web.Components.Pages.Daily.Habits;
using BeeDayLab.Web.Components.Pages.Daily.Projects;
using BeeDayLab.Web.Components.Pages.Daily.Tasks;
using BeeDayLab.Web.Components.Pages.Daily.Todos;
using BeeDayLab.Web.Scenarios;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Sprint 33.13 (FE33-088..097) checks for the two things this Sprint added alongside its
/// components: the six new resx catalog families (copied verbatim, resolved through the real
/// <see cref="IStringLocalizer{T}"/> pipeline in both supported cultures, matching the Sprint
/// 33.10/33.11/33.12 convention), and <see cref="DailyDashboardScenarioProvider"/>'s own determinism
/// and per-state contract.
/// </summary>
public sealed class DailyScenarioAndLocalizationTests
{
    // ------------------------------------------------------------------------------------------
    // The six new resx families, one representative key each, both cultures.
    // ------------------------------------------------------------------------------------------

    [Theory]
    [InlineData("en-US", "Habits")]
    [InlineData("pt-BR", "Hábitos")]
    public void DashboardResourcesResolvesForCulture(string culture, string expected) =>
        AssertLocalized<DashboardResources>(culture, "HabitsColumnTitle", expected);

    [Theory]
    [InlineData("en-US", "Easy")]
    [InlineData("pt-BR", "Fácil")]
    public void HabitResourcesResolvesForCulture(string culture, string expected) =>
        AssertLocalized<HabitResources>(culture, "DifficultyEasy", expected);

    [Theory]
    [InlineData("en-US", "Daily")]
    [InlineData("pt-BR", "Diariamente")]
    public void TaskResourcesResolvesForCulture(string culture, string expected) =>
        AssertLocalized<TaskResources>(culture, "RepeatDaily", expected);

    [Theory]
    [InlineData("en-US", "Project")]
    [InlineData("pt-BR", "Projeto")]
    public void TodoResourcesResolvesForCulture(string culture, string expected) =>
        AssertLocalized<TodoResources>(culture, "ProjectLabel", expected);

    [Theory]
    [InlineData("en-US", "In Progress")]
    [InlineData("pt-BR", "Em andamento")]
    public void ProjectResourcesResolvesForCulture(string culture, string expected) =>
        AssertLocalized<ProjectResources>(culture, "StatusInProgress", expected);

    [Theory]
    [InlineData("en-US", "Level")]
    [InlineData("pt-BR", "Nível")]
    public void ExperienceResourcesResolvesForCulture(string culture, string expected) =>
        AssertLocalized<ExperienceResources>(culture, "LevelLabel", expected);

    // ------------------------------------------------------------------------------------------
    // Provider contract.
    // ------------------------------------------------------------------------------------------

    [Theory]
    [InlineData(ScenarioState.Empty)]
    [InlineData(ScenarioState.Populated)]
    [InlineData(ScenarioState.Loading)]
    [InlineData(ScenarioState.Error)]
    [InlineData(ScenarioState.NoResults)]
    [InlineData(ScenarioState.Disabled)]
    [InlineData(ScenarioState.LargeContent)]
    [InlineData(ScenarioState.Selected)]
    public void GetScenarioIsATotalDeterministicFunctionOfItsContext(ScenarioState state)
    {
        var provider = new DailyDashboardScenarioProvider();
        var context = new ScenarioContext(state, "en-US");

        var first = provider.GetScenario(context);
        var second = provider.GetScenario(context);

        Assert.NotNull(first);

        // Same context, equal result — including every id, which is why the provider derives them
        // from fixed seeds instead of Guid.NewGuid().
        Assert.Equal(first.Profile, second.Profile);
        Assert.Equal(first.Habits, second.Habits);
        Assert.Equal(first.Tasks, second.Tasks);
        Assert.Equal(first.Projects, second.Projects);
        Assert.Equal(first.XpGainPerAction, second.XpGainPerAction);
    }

    [Fact]
    public void TwoSeparateProviderInstancesAgree()
    {
        var context = new ScenarioContext(ScenarioState.Populated, "en-US");

        var first = new DailyDashboardScenarioProvider().GetScenario(context);
        var second = new DailyDashboardScenarioProvider().GetScenario(context);

        Assert.Equal(first.Habits, second.Habits);
        Assert.Equal(first.Projects, second.Projects);
    }

    [Fact]
    public void EveryScenarioIdIsUniqueWithinItsCollection()
    {
        var scenario = new DailyDashboardScenarioProvider()
            .GetScenario(new ScenarioContext(ScenarioState.LargeContent, "en-US"));

        Assert.Equal(scenario.Habits.Count, scenario.Habits.Select(item => item.Id).Distinct().Count());
        Assert.Equal(scenario.Tasks.Count, scenario.Tasks.Select(item => item.Id).Distinct().Count());
        Assert.Equal(scenario.Projects.Count, scenario.Projects.Select(item => item.Id).Distinct().Count());

        var todos = scenario.Projects.SelectMany(project => project.Todos).ToList();
        Assert.Equal(todos.Count, todos.Select(item => item.Id).Distinct().Count());
    }

    [Fact]
    public void EveryNestedTodoBelongsToTheProjectItIsNestedUnder()
    {
        // Production's DashboardResponse nests todos per-project AND gives each one a ProjectId; the
        // Lab mirrors that shape, so the two must never disagree.
        var scenario = new DailyDashboardScenarioProvider()
            .GetScenario(new ScenarioContext(ScenarioState.Populated, "en-US"));

        foreach (var project in scenario.Projects)
        {
            Assert.All(project.Todos, todo => Assert.Equal(project.Id, todo.ProjectId));
        }
    }

    [Fact]
    public void TheXpGainIsTheSameFixedConstantForEveryContentScenario()
    {
        var provider = new DailyDashboardScenarioProvider();

        Assert.Equal(10, provider.GetScenario(new ScenarioContext(ScenarioState.Populated, "en-US")).XpGainPerAction);
        Assert.Equal(10, provider.GetScenario(new ScenarioContext(ScenarioState.Empty, "en-US")).XpGainPerAction);
        Assert.Equal(10, provider.GetScenario(new ScenarioContext(ScenarioState.LargeContent, "en-US")).XpGainPerAction);
    }

    [Fact]
    public void CultureDoesNotChangeTheSyntheticData()
    {
        var provider = new DailyDashboardScenarioProvider();

        var english = provider.GetScenario(new ScenarioContext(ScenarioState.Populated, "en-US"));
        var portuguese = provider.GetScenario(new ScenarioContext(ScenarioState.Populated, "pt-BR"));

        Assert.Equal(english.Habits, portuguese.Habits);
    }

    // ------------------------------------------------------------------------------------------
    // ExperienceViewModel — presentation formatting over already-resolved values.
    // ------------------------------------------------------------------------------------------

    [Fact]
    public void ExperienceViewModelProjectsTheProfileWithoutInventingAnyLevelingMath()
    {
        var profile = new DailyDashboardScenarioProvider()
            .GetScenario(new ScenarioContext(ScenarioState.Populated, "en-US"))
            .Profile;

        var model = ExperienceViewModel.From(profile);

        Assert.Equal(profile.CurrentLevel, model.Level);
        Assert.Equal(profile.CurrentLevelExperience, model.CurrentExperience);
        Assert.Equal(profile.ExperienceRequiredForCurrentLevel, model.RequiredExperience);
        Assert.Equal(profile.TotalExperience, model.TotalExperience);
        Assert.Equal(
            profile.ExperienceRequiredForCurrentLevel - profile.CurrentLevelExperience,
            model.RemainingExperience);
    }

    [Theory]
    [InlineData(0, 0, 100d)]   // No requirement -> treated as complete, never a divide-by-zero.
    [InlineData(50, 100, 50d)]
    [InlineData(500, 100, 100d)] // Clamped, so an over-full bar cannot overflow its track.
    public void ExperienceViewModelProgressPercentageIsAClampedRatio(long current, long required, double expected)
    {
        var model = new ExperienceViewModel(1, current, required, 0);

        Assert.Equal(expected, model.ProgressPercentage);
    }

    [Fact]
    public void ExperienceViewModelExposesNoDomainTypedFactory()
    {
        // Production's second factory, From(BeeDay.Domain.Experience.UserExperience), is deliberately
        // not ported — porting it would require a Domain type in the Lab.
        var factories = typeof(ExperienceViewModel)
            .GetMethods()
            .Where(method => method.Name == "From")
            .ToList();

        Assert.Single(factories);
        Assert.Equal(typeof(DailyUserProfileSummary), factories[0].GetParameters()[0].ParameterType);
    }

    // ------------------------------------------------------------------------------------------

    private static void AssertLocalized<TResources>(string culture, string key, string expected)
        where TResources : class
    {
        using var scope = new TestCultureScope(culture);

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None));
        services.AddLocalization();

        var localizer = services.BuildServiceProvider().GetRequiredService<IStringLocalizer<TResources>>();
        var value = localizer[key];

        Assert.False(value.ResourceNotFound, $"'{key}' not found in {typeof(TResources).Name} for {culture}.");
        Assert.Equal(expected, value.Value);
        Assert.Equal(culture, CultureInfo.CurrentUICulture.Name);
    }
}
