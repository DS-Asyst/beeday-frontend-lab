using BeeDayLab.Web.Scenarios;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Plain xUnit tests for the Sprint 33.10 (FE33-104, Issue #371) scenario engine — no bUnit
/// rendering needed, this is non-UI logic. Proves: deterministic lookup (same
/// <see cref="ScenarioContext"/> in, equal data out, called twice), the illustrative
/// <see cref="DemoCardListScenarioProvider"/> returns the documented shape per state, and
/// <see cref="ScenarioSelection"/>'s <c>Changed</c> event fires on an actual change and not on a
/// no-op set — mirroring <c>ToastService</c>'s tested no-op-safe <c>Remove</c> from Sprint 33.8.
/// </summary>
public sealed class ScenarioEngineTests
{
    [Theory]
    [InlineData(ScenarioState.Empty)]
    [InlineData(ScenarioState.Populated)]
    [InlineData(ScenarioState.Loading)]
    [InlineData(ScenarioState.Error)]
    [InlineData(ScenarioState.NoResults)]
    [InlineData(ScenarioState.Disabled)]
    [InlineData(ScenarioState.LargeContent)]
    [InlineData(ScenarioState.Selected)]
    public void GetScenarioIsDeterministicForEveryNamedState(ScenarioState state)
    {
        var provider = new DemoCardListScenarioProvider();
        var context = new ScenarioContext(state, "en-US");

        var first = provider.GetScenario(context);
        var second = provider.GetScenario(context);

        Assert.Equal(first, second);
        Assert.Same(first.Items, second.Items);
    }

    [Fact]
    public void GetScenarioIsDeterministicAcrossDifferentProviderInstances()
    {
        var context = new ScenarioContext(ScenarioState.Populated, "pt-BR", ViewportPreset.Mobile);

        var first = new DemoCardListScenarioProvider().GetScenario(context);
        var second = new DemoCardListScenarioProvider().GetScenario(context);

        Assert.Equal(first, second);
    }

    [Fact]
    public void EmptyStateReturnsNoItems()
    {
        var provider = new DemoCardListScenarioProvider();

        var data = provider.GetScenario(new ScenarioContext(ScenarioState.Empty, "en-US"));

        Assert.Empty(data.Items);
    }

    [Fact]
    public void NoResultsStateReturnsNoItems()
    {
        var provider = new DemoCardListScenarioProvider();

        var data = provider.GetScenario(new ScenarioContext(ScenarioState.NoResults, "en-US"));

        Assert.Empty(data.Items);
    }

    [Fact]
    public void PopulatedStateReturnsARealisticHandfulOfItems()
    {
        var provider = new DemoCardListScenarioProvider();

        var data = provider.GetScenario(new ScenarioContext(ScenarioState.Populated, "en-US"));

        Assert.InRange(data.Items.Count, 3, 12);
        Assert.All(data.Items, item =>
        {
            Assert.False(string.IsNullOrWhiteSpace(item.Title));
            Assert.False(string.IsNullOrWhiteSpace(item.Subtitle));
        });
    }

    [Fact]
    public void LargeContentStateReturnsFiftyOrMoreItems()
    {
        var provider = new DemoCardListScenarioProvider();

        var data = provider.GetScenario(new ScenarioContext(ScenarioState.LargeContent, "en-US"));

        Assert.True(data.Items.Count >= 50, $"Expected >= 50 items, got {data.Items.Count}.");
    }

    [Fact]
    public void ContextIsStructurallyEqualByValueNotReference()
    {
        var first = new ScenarioContext(ScenarioState.Populated, "en-US", ViewportPreset.Desktop);
        var second = new ScenarioContext(ScenarioState.Populated, "en-US", ViewportPreset.Desktop);

        Assert.Equal(first, second);
        Assert.NotSame(first, second);
    }

    [Fact]
    public void ScenarioSelectionDefaultsToPopulatedAndCurrentUiCulture()
    {
        var selection = new ScenarioSelection();

        Assert.Equal(ScenarioState.Populated, selection.State);
        Assert.Equal(System.Globalization.CultureInfo.CurrentUICulture.Name, selection.Culture);
        Assert.Null(selection.Viewport);
    }

    [Fact]
    public void ScenarioSelectionChangedFiresOnStateChange()
    {
        var selection = new ScenarioSelection();
        var changedCount = 0;
        selection.Changed += () => changedCount++;

        selection.State = ScenarioState.Empty;

        Assert.Equal(1, changedCount);
        Assert.Equal(ScenarioState.Empty, selection.State);
    }

    [Fact]
    public void ScenarioSelectionChangedDoesNotFireOnNoOpStateSet()
    {
        var selection = new ScenarioSelection { State = ScenarioState.Error };
        var changedCount = 0;
        selection.Changed += () => changedCount++;

        selection.State = ScenarioState.Error;

        Assert.Equal(0, changedCount);
    }

    [Fact]
    public void ScenarioSelectionChangedFiresOnViewportChangeButNotOnNoOp()
    {
        var selection = new ScenarioSelection();
        var changedCount = 0;
        selection.Changed += () => changedCount++;

        selection.Viewport = ViewportPreset.Mobile;
        selection.Viewport = ViewportPreset.Mobile;

        Assert.Equal(1, changedCount);
    }

    [Fact]
    public void ScenarioSelectionChangedFiresOnCultureChangeButNotOnNoOp()
    {
        var selection = new ScenarioSelection { Culture = "en-US" };
        var changedCount = 0;
        selection.Changed += () => changedCount++;

        selection.Culture = "pt-BR";
        selection.Culture = "pt-BR";

        Assert.Equal(1, changedCount);
        Assert.Equal("pt-BR", selection.Culture);
    }

    [Fact]
    public void ScenarioSelectionContextReflectsCurrentSelection()
    {
        var selection = new ScenarioSelection
        {
            State = ScenarioState.LargeContent,
            Viewport = ViewportPreset.Tablet,
            Culture = "pt-BR"
        };

        var context = selection.Context;

        Assert.Equal(new ScenarioContext(ScenarioState.LargeContent, "pt-BR", ViewportPreset.Tablet), context);
    }
}
