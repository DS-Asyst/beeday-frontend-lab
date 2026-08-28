using BeeDayLab.Web.Components.Pages.Wallet.Services;
using BeeDayLab.Web.Components.Pages.Wallet.State;
using BeeDayLab.Web.Scenarios;
using Xunit;

namespace BeeDayLab.Web.Tests;

public sealed class WalletScenarioAndStateTests
{
    private static readonly ScenarioContext Context = new(ScenarioState.Populated, "en-US", null);

    [Fact]
    public void ProviderIsDeterministicAndCoversEveryScenarioShape()
    {
        var provider = new WalletScenarioProvider();

        var first = provider.GetScenario(Context);
        var second = provider.GetScenario(Context);
        var empty = provider.GetScenario(Context with { State = ScenarioState.Empty });
        var large = provider.GetScenario(Context with { State = ScenarioState.LargeContent });
        var noResults = provider.GetScenario(Context with { State = ScenarioState.NoResults });

        Assert.Equal(first.Summary, second.Summary);
        Assert.Equal(first.Tags, second.Tags);
        Assert.Equal(first.Transactions, second.Transactions);
        Assert.Equal(12, first.Transactions.Count);
        Assert.Equal(5, first.Tags.Count);
        Assert.Empty(empty.Transactions);
        Assert.Empty(empty.Tags);
        Assert.Equal(45, large.Transactions.Count);
        Assert.Equal(first.Transactions, noResults.Transactions);
    }

    [Fact]
    public void SummaryValuesAreStableScenarioResolvedDisplayValues()
    {
        var data = new WalletScenarioProvider().GetScenario(Context);

        Assert.Equal(4_285.40m, data.Summary.Balance);
        Assert.Equal(8_950.00m, data.Summary.TotalIncome);
        Assert.Equal(4_664.60m, data.Summary.TotalExpenses);
        Assert.Equal(12, data.Summary.TransactionCount);
    }

    [Fact]
    public void PageStateCountsAndClearsEveryFilterWithoutChangingSort()
    {
        var state = new WalletPageState
        {
            Search = "rent",
            TypeFilter = "Expense",
            TagFilter = "00000000-0000-0000-0000-000000000001",
            StartDate = new DateOnly(2026, 8, 1),
            EndDate = new DateOnly(2026, 8, 31),
            Sort = "amount-desc",
            Page = 3,
        };

        Assert.Equal(5, state.ActiveFilterCount);
        state.ClearFilters();

        Assert.False(state.HasFilters);
        Assert.Equal(1, state.Page);
        Assert.Equal("amount-desc", state.Sort);
    }

    [Fact]
    public void InteractionStatePreventsConcurrentMutations()
    {
        var state = new WalletInteractionState();

        Assert.True(state.TryBegin("save-transaction"));
        Assert.False(state.TryBegin("delete-tag"));
        Assert.Equal("save-transaction", state.Operation);

        state.End();
        Assert.False(state.IsBusy);
        Assert.Null(state.Operation);
    }

    [Theory]
    [InlineData("en-US", "$1,234.56")]
    [InlineData("pt-BR", "$ 1.234,56")]
    public void CurrencyFormattingChangesPresentationButKeepsUsd(string culture, string expected)
    {
        using var scope = new TestCultureScope(culture);
        Assert.Equal(expected, WalletCurrencyFormatter.Format(1_234.56m));
    }

    [Theory]
    [InlineData("#FFFFFF", "#17111f")]
    [InlineData("#100F3E", "#ffffff")]
    [InlineData("invalid", "#ffffff")]
    public void TagContrastUsesTheProductionPresentationRule(string color, string expected) =>
        Assert.Equal(expected, TagContrastCalculator.GetTextColor(color));
}
