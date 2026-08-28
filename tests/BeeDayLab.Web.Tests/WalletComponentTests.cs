using BeeDayLab.Web.Components.Pages.Wallet.Components;
using BeeDayLab.Web.Components.Pages.Wallet.Models;
using BeeDayLab.Web.Scenarios;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BeeDayLab.Web.Tests;

public sealed class WalletComponentTests
{
    private static readonly WalletScenarioData Scenario = new WalletScenarioProvider()
        .GetScenario(new ScenarioContext(ScenarioState.Populated, "en-US", null));

    [Theory]
    [InlineData("en-US", "Current balance", "$4,285.40")]
    [InlineData("pt-BR", "Saldo atual", "$ 4.285,40")]
    public void SummaryUsesLocalizedLabelsAndCultureAwareUsdFormatting(string culture, string label, string amount)
    {
        using var scope = new TestCultureScope(culture);
        using var context = CreateContext();
        var cut = context.Render<WalletSummary>(parameters => parameters.Add(item => item.Summary, Scenario.Summary));

        Assert.Contains(label, cut.Markup, StringComparison.Ordinal);
        Assert.Contains(amount, cut.Markup, StringComparison.Ordinal);
        Assert.Equal(3, cut.FindAll(".wallet-summary__card").Count);
    }

    [Theory]
    [InlineData("en-US", "7/1/2026")]
    [InlineData("pt-BR", "01/07/2026")]
    public void TransactionCardUsesLocalizedShortDateAndReadableTagContrast(string culture, string expectedDate)
    {
        using var scope = new TestCultureScope(culture);
        using var context = CreateContext();
        var transaction = Scenario.Transactions[1] with
        {
            TransactionDate = new DateOnly(2026, 7, 1),
            WalletTagColor = "#100F3E",
        };
        var cut = context.Render<TransactionCard>(parameters => parameters.Add(item => item.Transaction, transaction));

        Assert.Equal(expectedDate, cut.Find("time").TextContent);
        Assert.Equal("2026-07-01", cut.Find("time").GetAttribute("datetime"));
        Assert.Contains("color:#ffffff", cut.Find(".wallet-tag-badge").GetAttribute("style"), StringComparison.Ordinal);
    }

    [Fact]
    public void FiltersExposeSearchTagTypeDatesAndAllSixSortOptions()
    {
        using var context = CreateContext();
        var cut = context.Render<WalletFilters>(parameters => parameters
            .Add(item => item.TypeFilter, "Expense")
            .Add(item => item.ActiveFilterCount, 1)
            .Add(item => item.Tags, Scenario.Tags));

        Assert.Equal(6, cut.FindAll("input, select").Count);
        Assert.Equal(6, cut.FindAll("#wallet-sort-filter option").Count);
        Assert.Equal("true", cut.Find(".wallet-filter-toggle").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void TransactionAndTagDialogsRenderCreateEditDeleteAndInlineErrorStates()
    {
        using var context = CreateContext();

        var transaction = context.Render<TransactionFormModal>(parameters => parameters
            .Add(item => item.IsOpen, true)
            .Add(item => item.IsEditing, true)
            .Add(item => item.ErrorMessage, "Synthetic transaction error")
            .Add(item => item.Model, new TransactionFormModel())
            .Add(item => item.Tags, Scenario.Tags));

        Assert.Contains("Synthetic transaction error", transaction.Find(".editor-modal__error").TextContent, StringComparison.Ordinal);
        Assert.NotEmpty(transaction.FindAll(".editor-modal__footer-danger button"));

        var tag = context.Render<TagFormModal>(parameters => parameters
            .Add(item => item.IsOpen, true)
            .Add(item => item.IsEditing, false)
            .Add(item => item.ErrorMessage, "Synthetic tag error")
            .Add(item => item.Model, new WalletTagFormModel()));

        Assert.Contains("Synthetic tag error", tag.Find(".editor-modal__error").TextContent, StringComparison.Ordinal);
        Assert.Equal(10, tag.FindAll(".wallet-color-swatch").Count);
        Assert.Empty(tag.FindAll("input[type='color']"));
    }

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddLocalization();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }
}
