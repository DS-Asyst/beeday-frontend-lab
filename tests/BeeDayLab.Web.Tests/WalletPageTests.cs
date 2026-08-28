using BeeDayLab.Web.Components.DesignSystem.Feedback;
using BeeDayLab.Web.Scenarios;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using WalletPage = BeeDayLab.Web.Components.Pages.Wallet.Wallet;

namespace BeeDayLab.Web.Tests;

public sealed class WalletPageTests
{
    [Fact]
    public void LoadingErrorEmptyAndNoResultsAreDistinctStates()
    {
        using var culture = new TestCultureScope();

        using (var loading = CreateContext(ScenarioState.Loading))
        {
            var cut = loading.Render<WalletPage>();
            Assert.NotEmpty(cut.FindAll("section.dashboard-skeleton"));
        }

        using (var error = CreateContext(ScenarioState.Error))
        {
            var cut = error.Render<WalletPage>();
            Assert.Equal("alert", cut.Find(".wallet-alert").GetAttribute("role"));
        }

        using (var empty = CreateContext(ScenarioState.Empty))
        {
            var cut = empty.Render<WalletPage>();
            Assert.NotEmpty(cut.FindAll(".wallet-empty-state"));
            Assert.Empty(cut.FindAll(".wallet-transaction-card"));
            Assert.Empty(cut.FindAll(".wallet-tag-item"));
        }

        using (var noResults = CreateContext(ScenarioState.NoResults))
        {
            var cut = noResults.Render<WalletPage>();
            Assert.NotEmpty(cut.FindAll(".wallet-empty-state"));
            Assert.NotEmpty(cut.FindAll(".wallet-empty-state .beeday-button--confirmation-cancel"));
            Assert.Contains("Clear", cut.Find(".wallet-empty-state").TextContent, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void PopulatedLargeAndDisabledStatesPreserveCollectionAndInteractionContracts()
    {
        using var culture = new TestCultureScope();

        using (var populated = CreateContext(ScenarioState.Populated))
        {
            var cut = populated.Render<WalletPage>();
            Assert.Equal(12, cut.FindAll(".wallet-transaction-card").Count);
            Assert.Equal(5, cut.FindAll(".wallet-tag-item").Count);
        }

        using (var large = CreateContext(ScenarioState.LargeContent))
        {
            var cut = large.Render<WalletPage>();
            Assert.Equal(20, cut.FindAll(".wallet-transaction-card").Count);
            Assert.NotEmpty(cut.FindAll(".wallet-pagination"));
            Assert.Contains("45", cut.Find(".wallet-result-count").TextContent, StringComparison.Ordinal);
        }

        using (var disabled = CreateContext(ScenarioState.Disabled))
        {
            var cut = disabled.Render<WalletPage>();
            Assert.NotEmpty(cut.FindAll("button"));
            Assert.All(cut.FindAll("button"), button => Assert.True(button.HasAttribute("disabled")));
            Assert.All(cut.FindAll("input, select"), field => Assert.True(field.HasAttribute("disabled")));
            Assert.All(cut.FindAll(".wallet-tag-item"), tag =>
            {
                Assert.Equal("true", tag.GetAttribute("aria-disabled"));
                Assert.Equal("-1", tag.GetAttribute("tabindex"));
            });
        }
    }

    [Theory]
    [InlineData(ScenarioState.Selected, null, ".editor-modal")]
    [InlineData(ScenarioState.Populated, "transaction-create", ".editor-modal")]
    [InlineData(ScenarioState.Populated, "transaction-delete", ".delete-confirmation")]
    [InlineData(ScenarioState.Populated, "tag-create", ".editor-modal")]
    [InlineData(ScenarioState.Populated, "tag-delete-error", ".delete-confirmation__error")]
    public void DialogPreviewStatesAreDirectlySelectable(ScenarioState state, string? dialog, string selector)
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext(state);
        if (dialog is not null)
        {
            var navigation = context.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
            navigation.NavigateTo($"{navigation.BaseUri}wallet?dialog={Uri.EscapeDataString(dialog)}");
        }

        var cut = context.Render<WalletPage>();

        Assert.NotEmpty(cut.FindAll(selector));
    }

    private static BunitContext CreateContext(ScenarioState state)
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddLocalization();
        context.Services.AddScoped<ToastService>();
        context.Services.AddScoped(_ => new ScenarioSelection { State = state });
        context.Services.AddSingleton<WalletScenarioProvider>();

        var dialogFocus = context.JSInterop.SetupModule("./js/beeday-dialog-focus.js");
        dialogFocus.SetupVoid("deactivate", _ => true);
        dialogFocus.Setup<bool>("activate", _ => true).SetResult(true);
        dialogFocus.SetupVoid("focusFirstInvalid", _ => true);
        return context;
    }
}
