using BeeDayLab.Web.Components.DesignSystem.Buttons;
using BeeDayLab.Web.Components.DesignSystem.Feedback;
using BeeDayLab.Web.Components.DesignSystem.Icons;
using BeeDayLab.Web.Scenarios;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using GalleryPage = BeeDayLab.Web.Components.Pages.Gallery.ComponentGallery;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Sprint 33.16 (Issue #377): the gallery must render REAL component instances with their mapped
/// variants/states, not screenshots — every assertion below targets markup only the actual
/// Design System component can produce (its own CSS class names, its own ARIA contract), never a
/// hand-authored lookalike. Every test that asserts English chrome text uses <see cref="TestCultureScope"/>
/// so results are deterministic regardless of the host machine/CI runner's own OS culture.
/// </summary>
public sealed class ComponentGalleryTests
{
    [Fact]
    public void EveryButtonVariantRendersAsARealBeeDayButtonAndDisabledToggleAppliesToAll()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext();
        var cut = context.Render<GalleryPage>();

        foreach (var variant in Enum.GetValues<BeeDayButtonVariant>())
        {
            var expectedClass = $"beeday-button--{ToKebabCase(variant.ToString())}";
            Assert.Contains(cut.FindAll("button"), b => b.ClassList.Contains(expectedClass));
        }

        var disabledToggle = cut.Find("#gallery-buttons input[type=checkbox]");
        disabledToggle.Change(true);

        var swatchButtons = cut.Find(".gallery-page__swatches").QuerySelectorAll("button");
        Assert.NotEmpty(swatchButtons);
        Assert.All(swatchButtons, button => Assert.True(button.HasAttribute("disabled")));
    }

    [Fact]
    public void AllFourCardVariantsAreRealBeeDayCardInstances()
    {
        using var context = CreateContext();
        var cut = context.Render<GalleryPage>();

        var cards = cut.Find(".gallery-page__cards").QuerySelectorAll("article");
        Assert.Equal(4, cards.Length);
        Assert.Contains(cards, c => c.ClassList.Contains("beeday-card--muted"));
        Assert.Contains(cards, c => c.ClassList.Contains("beeday-card--prominent"));
        Assert.Contains(cards, c => c.ClassList.Contains("beeday-card--interactive"));
    }

    [Fact]
    public void LoadingOverlayTogglesTheRealBeeDayLoadingComponent()
    {
        using var context = CreateContext();
        var cut = context.Render<GalleryPage>();

        Assert.Empty(cut.FindAll(".beeday-loading-overlay"));

        cut.Find("#gallery-feedback input[type=checkbox]").Change(true);

        Assert.NotEmpty(cut.FindAll(".beeday-loading-overlay"));
    }

    [Fact]
    public void ErrorBoundaryDemoRendersTheRealFallbackContentWhenTriggered()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext();
        var cut = context.Render<GalleryPage>();

        var section = cut.Find("#gallery-feedback");
        var trigger = section.QuerySelectorAll("button")
            .Single(b => b.TextContent.Contains("Trigger a render error", StringComparison.Ordinal));

        trigger.Click();

        Assert.NotEmpty(cut.FindAll(".beeday-error-boundary"));
        Assert.Contains("Something went wrong", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void ToastButtonsPushRealMessagesThroughTheInjectedToastService()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext();
        var toastService = context.Services.GetRequiredService<ToastService>();
        var cut = context.Render<GalleryPage>();

        var successButton = cut.FindAll("button").Single(b => b.TextContent.Contains("Trigger success toast", StringComparison.Ordinal));
        successButton.Click();

        Assert.Single(toastService.Messages);
        Assert.NotEmpty(cut.FindAll(".beeday-toast"));
    }

    [Theory]
    [InlineData("Open confirm dialog", false, false)]
    [InlineData("Open confirm dialog (busy)", true, false)]
    [InlineData("Open confirm dialog (error)", false, true)]
    public void ConfirmDialogOpensWithTheRequestedBusyOrErrorState(string buttonText, bool expectBusy, bool expectError)
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext();
        var cut = context.Render<GalleryPage>();

        cut.FindAll("button").Single(b => b.TextContent.Trim() == buttonText).Click();

        var dialog = cut.Find(".delete-confirmation");
        Assert.NotNull(dialog);
        Assert.Equal(expectBusy, cut.FindAll(".delete-confirmation button[disabled]").Count > 0);
        if (expectError)
        {
            Assert.Contains("We could not delete the item", cut.Markup, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task SubmittingTheDemoFormWithoutARequiredFieldShowsARealValidationMessage()
    {
        using var context = CreateContext();
        var cut = context.Render<GalleryPage>();

        var form = cut.Find("#gallery-forms form");
        await form.SubmitAsync();

        Assert.NotEmpty(cut.FindAll(".beeday-validation-message"));
    }

    [Fact]
    public void IconSectionRendersOneSwatchPerSizeColorAndEveryMappedIconName()
    {
        using var context = CreateContext();
        var cut = context.Render<GalleryPage>();

        var expected = Enum.GetValues<BeeDayIconSize>().Length
            + Enum.GetValues<BeeDayIconColor>().Length
            + Enum.GetValues<BeeDayIconName>().Length;

        var actual = cut.Find("#gallery-icons").QuerySelectorAll(".gallery-page__icon-swatch").Length;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void LayoutSectionRendersRealHeroPageHeaderAndSectionHeaderInstances()
    {
        using var context = CreateContext();
        var cut = context.Render<GalleryPage>();

        var section = cut.Find("#gallery-layout");
        Assert.Equal(2, section.QuerySelectorAll(".beeday-hero").Length);
        Assert.NotEmpty(section.QuerySelectorAll(".beeday-page-header"));
        Assert.NotEmpty(section.QuerySelectorAll(".beeday-settings-section"));
    }

    [Fact]
    public void EditorModalOpensWithTheRealEditorModalShellAndClosesOnSubmit()
    {
        using var context = CreateContext();
        var cut = context.Render<GalleryPage>();

        cut.Find("#gallery-modals button").Click();
        Assert.NotEmpty(cut.FindAll(".editor-modal"));

        var saveButton = cut.FindAll(".editor-modal button[type=submit]").Single();
        saveButton.Click();

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll(".editor-modal")), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ProgressSectionRendersDeterminateAndUnavailableStatesFromTheRealComponent()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext();
        var cut = context.Render<GalleryPage>();

        var bars = cut.Find("#gallery-progress").QuerySelectorAll("[role='progressbar']");
        Assert.Equal(3, bars.Length);
        Assert.Contains(bars, b => b.GetAttribute("aria-valuetext") == "Progress unavailable");
    }

    [Fact]
    public void TextSectionRendersBrandTwiceAndHighlightsTheTypedSearchTerm()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext();
        var cut = context.Render<GalleryPage>();

        var section = cut.Find("#gallery-text");
        Assert.Equal(2, section.QuerySelectorAll(".beeday-brand").Length);

        var input = section.QuerySelector("input[type=text]")!;
        input.Input("track");

        Assert.NotEmpty(cut.FindAll("#gallery-text mark"));
    }

    [Fact]
    public void SortableSectionRendersARealListItemPerDemoCardWithTheGalleryCollectionKey()
    {
        using var culture = new TestCultureScope();
        using var context = CreateContext();
        var cut = context.Render<GalleryPage>();

        var list = cut.Find("#gallery-behaviors [role='list']");
        Assert.Equal("gallery-demo", list.GetAttribute("data-sortable-key"));
        Assert.Equal(3, cut.Find("#gallery-behaviors").QuerySelectorAll("[role='listitem']").Length);
        Assert.Contains("First demo card", cut.Markup, StringComparison.Ordinal);
    }

    private static string ToKebabCase(string pascalCase) =>
        string.Concat(pascalCase.Select((c, i) => i > 0 && char.IsUpper(c) ? $"-{c}" : c.ToString())).ToLowerInvariant();

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddLogging();
        context.Services.AddLocalization();
        context.Services.AddScoped<ToastService>();
        context.Services.AddScoped(_ => new ScenarioSelection());

        var dialogFocus = context.JSInterop.SetupModule("./js/beeday-dialog-focus.js");
        dialogFocus.SetupVoid("deactivate", _ => true);
        dialogFocus.Setup<bool>("activate", _ => true).SetResult(true);
        dialogFocus.SetupVoid("focusFirstInvalid", _ => true);

        var sortable = context.JSInterop.SetupModule("./js/beeday-sortable.js");
        sortable.SetupVoid("initialize", _ => true);
        sortable.SetupVoid("dispose", _ => true);

        return context;
    }
}
