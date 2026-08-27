using BeeDayLab.Web.Components.DesignSystem.Buttons;
using BeeDayLab.Web.Components.DesignSystem.Feedback;
using BeeDayLab.Web.Components.DesignSystem.Icons;
using BeeDayLab.Web.Components.DesignSystem.Progress;
using BeeDayLab.Web.Components.DesignSystem.Text;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Deterministic bUnit component tests for Sprint 33.8 (FE33-013..029): proves the shared,
/// non-form Design System components copied/adapted from DS-Asyst/BeeDay (baseline acce26a) render
/// their variants/states and accessibility attributes without needing any production service
/// (BeeDayWebService, ISender, real auth, EF Core) — ADR-008's "zero BeeDay-specific dependency"
/// requirement made testable.
/// </summary>
public sealed class SharedComponentsParityTests
{
    [Theory]
    [InlineData(BeeDayButtonVariant.Primary)]
    [InlineData(BeeDayButtonVariant.Secondary)]
    [InlineData(BeeDayButtonVariant.Success)]
    [InlineData(BeeDayButtonVariant.Warning)]
    [InlineData(BeeDayButtonVariant.Back)]
    [InlineData(BeeDayButtonVariant.Danger)]
    [InlineData(BeeDayButtonVariant.ConfirmationDanger)]
    [InlineData(BeeDayButtonVariant.ConfirmationCancel)]
    [InlineData(BeeDayButtonVariant.ImportantWhite)]
    public void EveryButtonVariantRendersItsOwnModifierClass(BeeDayButtonVariant variant)
    {
        using var context = new BunitContext();

        var cut = context.Render<BeeDayButton>(parameters => parameters
            .Add(p => p.Variant, variant)
            .AddChildContent("Click me"));

        var expectedClass = variant switch
        {
            BeeDayButtonVariant.Primary => "beeday-button--primary",
            BeeDayButtonVariant.Secondary => "beeday-button--secondary",
            BeeDayButtonVariant.Success => "beeday-button--success",
            BeeDayButtonVariant.Warning => "beeday-button--warning",
            BeeDayButtonVariant.Back => "beeday-button--back",
            BeeDayButtonVariant.Danger => "beeday-button--danger",
            BeeDayButtonVariant.ConfirmationDanger => "beeday-button--confirmation-danger",
            BeeDayButtonVariant.ConfirmationCancel => "beeday-button--confirmation-cancel",
            BeeDayButtonVariant.ImportantWhite => "beeday-button--important-white",
            _ => throw new ArgumentOutOfRangeException(nameof(variant))
        };

        var button = cut.Find("button");
        Assert.Contains(expectedClass, button.ClassList);
    }

    [Fact]
    public void LoadingButtonIsDisabledAndAriaBusy()
    {
        using var context = new BunitContext();

        var cut = context.Render<BeeDayButton>(parameters => parameters
            .Add(p => p.IsLoading, true)
            .AddChildContent("Save"));

        var button = cut.Find("button");
        Assert.True(button.HasAttribute("disabled"));
        Assert.Equal("true", button.GetAttribute("aria-busy"));
        Assert.Contains("beeday-button--loading", button.ClassList);
    }

    [Fact]
    public void DisabledButtonNeverInvokesOnClick()
    {
        using var context = new BunitContext();
        var clicked = false;

        var cut = context.Render<BeeDayButton>(parameters => parameters
            .Add(p => p.Disabled, true)
            .Add(p => p.OnClick, EventCallback.Factory.Create(this, () => clicked = true))
            .AddChildContent("Save"));

        cut.Find("button").Click();

        Assert.False(clicked);
    }

    [Fact]
    public void EmptyStateRendersRoleStatusWithTitleAndDescription()
    {
        using var context = new BunitContext();

        var cut = context.Render<BeeDayEmptyState>(parameters => parameters
            .Add(p => p.Title, "Nothing here yet")
            .Add(p => p.Description, "Add your first item to get started.")
            .Add(p => p.Icon, BeeDayIconName.Search));

        var root = cut.Find(".beeday-empty-state");
        Assert.Equal("status", root.GetAttribute("role"));
        Assert.Equal("Nothing here yet", cut.Find(".beeday-empty-state__title").TextContent);
        Assert.Equal("Add your first item to get started.", cut.Find(".beeday-empty-state__description").TextContent);
    }

    [Fact]
    public void DashboardSkeletonIsAriaBusyWithDefaultLabel()
    {
        using var context = new BunitContext();

        var cut = context.Render<BeeDayDashboardSkeleton>();

        var section = cut.Find("section.dashboard-skeleton");
        Assert.Equal("true", section.GetAttribute("aria-busy"));
        Assert.Equal("Loading dashboard", section.GetAttribute("aria-label"));
    }

    [Fact]
    public void SkeletonRendersRequestedNumberOfLinesAndHidesFromAssistiveTech()
    {
        using var context = new BunitContext();

        var cut = context.Render<BeeDaySkeleton>(parameters => parameters
            .Add(p => p.Lines, 4));

        var root = cut.Find(".beeday-skeleton");
        Assert.Equal("true", root.GetAttribute("aria-hidden"));
        Assert.Equal(4, cut.FindAll(".beeday-skeleton__line").Count);
    }

    [Fact]
    public void ErrorBoundaryRendersBrandedFallbackWithRoleAlertWhenChildThrows()
    {
        using var context = new BunitContext();

        var cut = context.Render<BeeDayErrorBoundary>(parameters => parameters
            .AddChildContent<ThrowingComponent>());

        var alert = cut.Find("[role='alert']");
        Assert.Contains("Something went wrong", alert.TextContent);
        Assert.Contains("Reload page", alert.TextContent);
    }

    [Fact]
    public void LoadingOverlayIsPolitelyAnnouncedWithDefaultLabelWhenVisible()
    {
        using var context = new BunitContext();

        var cut = context.Render<BeeDayLoading>(parameters => parameters
            .Add(p => p.IsVisible, true));

        var region = cut.Find(".beeday-loading-overlay");
        Assert.Equal("status", region.GetAttribute("role"));
        Assert.Equal("polite", region.GetAttribute("aria-live"));
        Assert.Equal("Saving changes...", region.GetAttribute("aria-label"));
    }

    [Fact]
    public void LoadingOverlayRendersNothingWhenNotVisible()
    {
        using var context = new BunitContext();

        var cut = context.Render<BeeDayLoading>(parameters => parameters
            .Add(p => p.IsVisible, false));

        Assert.Empty(cut.Markup.Trim());
    }

    [Theory]
    [InlineData(0d, 100d, "empty")]
    [InlineData(50d, 100d, "partial")]
    [InlineData(100d, 100d, "complete")]
    [InlineData(1d, 0d, "unavailable")]
    public void ProgressBarStateReflectsValueAgainstMaximum(double value, double maximum, string expectedState)
    {
        using var context = new BunitContext();

        var cut = context.Render<BeeDayProgressBar>(parameters => parameters
            .Add(p => p.Label, "XP")
            .Add(p => p.Value, value)
            .Add(p => p.Maximum, maximum));

        var root = cut.Find(".beeday-progress");
        Assert.Equal(expectedState, root.GetAttribute("data-state"));

        var track = cut.Find("[role='progressbar']");
        Assert.Equal("0", track.GetAttribute("aria-valuemin"));
    }

    [Fact]
    public void ProgressBarUnavailableStateHasHardcodedEnglishFallback()
    {
        using var context = new BunitContext();

        var cut = context.Render<BeeDayProgressBar>(parameters => parameters
            .Add(p => p.Label, "XP")
            .Add(p => p.Value, 10d)
            .Add(p => p.Maximum, 0d));

        var track = cut.Find("[role='progressbar']");
        Assert.Equal("Progress unavailable", track.GetAttribute("aria-valuetext"));
    }

    [Fact]
    public void RewardToneUsesItsOwnDataAttributeForCssTargeting()
    {
        using var context = new BunitContext();

        var cut = context.Render<BeeDayProgressBar>(parameters => parameters
            .Add(p => p.Label, "Streak")
            .Add(p => p.Value, 3d)
            .Add(p => p.Maximum, 10d)
            .Add(p => p.Tone, BeeDayProgressTone.Reward));

        Assert.Equal("reward", cut.Find(".beeday-progress").GetAttribute("data-tone"));
    }

    [Fact]
    public void BrandRendersRoleImgWithBeedayAccessibleName()
    {
        using var context = new BunitContext();

        var cut = context.Render<BeeDayBrand>();

        var root = cut.Find("[role='img']");
        Assert.Equal("beeday", root.GetAttribute("aria-label"));
        Assert.Equal("bee", cut.Find(".beeday-brand__bee").TextContent);
        Assert.Equal("day", cut.Find(".beeday-brand__day").TextContent);
    }

    [Fact]
    public void BrandOnDarkSurfaceUsesTheNonWhiteBackgroundIcon()
    {
        using var context = new BunitContext();

        var cut = context.Render<BeeDayBrand>(parameters => parameters
            .Add(p => p.OnDarkSurface, true));

        Assert.Contains("beeday-brand--inverse", cut.Find(".beeday-brand").ClassList);
        Assert.Contains("bee-color-neutral.png", cut.Find("img").GetAttribute("src"));
    }

    [Fact]
    public void SearchHighlightWrapsMatchingSegmentInMarkCaseInsensitively()
    {
        using var context = new BunitContext();

        var cut = context.Render<SearchHighlight>(parameters => parameters
            .Add(p => p.Text, "Morning Routine")
            .Add(p => p.SearchTerm, "routine"));

        var mark = cut.Find("mark");
        Assert.Equal("Routine", mark.TextContent);
        Assert.Contains("beeday-search-highlight", mark.ClassList);
    }

    [Fact]
    public void SearchHighlightWithEmptyTermRendersPlainText()
    {
        using var context = new BunitContext();

        var cut = context.Render<SearchHighlight>(parameters => parameters
            .Add(p => p.Text, "Morning Routine")
            .Add(p => p.SearchTerm, string.Empty));

        Assert.Empty(cut.FindAll("mark"));
        Assert.Equal("Morning Routine", cut.Markup.Trim());
    }

    [Fact]
    public void ConfirmDialogClosedRendersNothingButKeepsFocusScopeInactive()
    {
        using var context = new BunitContext();

        var cut = context.Render<BeeDayConfirmDialog>(parameters => parameters
            .Add(p => p.IsOpen, false)
            .Add(p => p.Title, "Delete this?")
            .Add(p => p.Message, "This cannot be undone."));

        Assert.Empty(cut.FindAll(".delete-confirmation"));
    }

    [Fact]
    public void ConfirmDialogOpenRendersAlertdialogWithHardcodedEnglishDefaultLabels()
    {
        using var context = new BunitContext();
        SetupDialogFocusModule(context);

        var cut = context.Render<BeeDayConfirmDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Delete this?")
            .Add(p => p.Message, "This cannot be undone."));

        var dialog = cut.Find(".delete-confirmation");
        Assert.Equal("alertdialog", dialog.GetAttribute("role"));
        Assert.Contains("Cancel", cut.Markup);
        Assert.Contains("Confirm", cut.Markup);
    }

    [Fact]
    public void ConfirmDialogConfirmInvokesOnConfirmCallback()
    {
        using var context = new BunitContext();
        SetupDialogFocusModule(context);
        var confirmed = false;

        var cut = context.Render<BeeDayConfirmDialog>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Delete this?")
            .Add(p => p.Message, "This cannot be undone.")
            .Add(p => p.OnConfirm, EventCallback.Factory.Create(this, () => confirmed = true)));

        cut.Find(".delete-confirmation__confirm-action").Click();

        Assert.True(confirmed);
    }

    [Fact]
    public void ToastServiceHasZeroBeeDaySpecificDependenciesAndUsesHardcodedDefaultTitles()
    {
        var service = new ToastService();

        service.ShowSuccess("Saved.");
        service.ShowError("Could not save.");
        service.ShowInfo("Heads up.");

        Assert.Collection(
            service.Messages,
            toast => Assert.Equal("Success", toast.Title),
            toast => Assert.Equal("Something went wrong", toast.Title),
            toast => Assert.Equal("Information", toast.Title));
    }

    [Fact]
    public void ToastServiceRemoveRaisesChangedAndDropsTheMessage()
    {
        var service = new ToastService();
        var changedCount = 0;
        service.Changed += () => changedCount++;

        service.ShowSuccess("Saved.");
        var id = service.Messages[0].Id;
        service.Remove(id);

        Assert.Empty(service.Messages);
        Assert.Equal(2, changedCount);
    }

    [Fact]
    public void ToastHostRendersAriaLiveRegionWithHardcodedEnglishLabelsAndVariantRoles()
    {
        using var context = new BunitContext();
        var service = new ToastService();
        context.Services.AddSingleton(service);
        service.ShowSuccess("Saved.");
        service.ShowError("Failed.");

        var cut = context.Render<BeeDayToastHost>();

        var region = cut.Find(".beeday-toast-region");
        Assert.Equal("polite", region.GetAttribute("aria-live"));
        Assert.Equal("Notifications", region.GetAttribute("aria-label"));

        var toasts = cut.FindAll(".beeday-toast");
        Assert.Equal(2, toasts.Count);
        Assert.Equal("status", toasts[0].GetAttribute("role"));
        Assert.Equal("alert", toasts[1].GetAttribute("role"));

        Assert.Equal("Dismiss notification", cut.Find(".beeday-toast__close").GetAttribute("aria-label"));
    }

    [Fact]
    public void ToastHostDismissButtonRemovesTheToastFromTheService()
    {
        using var context = new BunitContext();
        var service = new ToastService();
        context.Services.AddSingleton(service);
        service.ShowInfo("Heads up.");

        var cut = context.Render<BeeDayToastHost>();
        cut.Find(".beeday-toast__close").Click();

        Assert.Empty(service.Messages);
        Assert.Empty(cut.FindAll(".beeday-toast"));
    }

    private sealed class ThrowingComponent : ComponentBase
    {
        protected override void OnInitialized() => throw new InvalidOperationException("boom");
    }

    private static void SetupDialogFocusModule(BunitContext context)
    {
        var module = context.JSInterop.SetupModule("./js/beeday-dialog-focus.js");
        module.SetupVoid("deactivate", _ => true);
        module.Setup<bool>("activate", _ => true).SetResult(true);
    }
}
