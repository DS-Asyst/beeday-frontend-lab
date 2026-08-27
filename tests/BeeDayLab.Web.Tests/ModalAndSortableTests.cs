using BeeDayLab.Web.Components.Behaviors.DragDrop;
using BeeDayLab.Web.Components.DesignSystem.Modals;
using Bunit;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Deterministic bUnit component tests for Sprint 33.8 (FE33-027..029, FE33-106/107): proves the
/// two JS-interop-backed Modals components (DialogFocusScope, EditorModalShell) and the drag/drop
/// behavior (BeeDaySortable) render and wire their callbacks correctly with the query-string-free
/// "./js/beeday-*.js" import paths this Sprint deliberately adopted (dropping the hardcoded
/// "?v=..." cache-busting suffixes the BeeDay source carries — documented quality debt, not
/// behavior to preserve), and that BeeDaySortable's OnReorder callback (its only degree of freedom)
/// works against a plain local test handler since no Lab consumer page wires it to a real reorder
/// service yet (that arrives in a later gallery Sprint).
/// </summary>
public sealed class ModalAndSortableTests
{
    [Fact]
    public void DialogFocusScopeImportsTheQueryStringFreeModulePathWhenActive()
    {
        using var context = new BunitContext();
        SetupDialogFocusModule(context);

        context.Render<DialogFocusScope>(parameters => parameters
            .Add(p => p.Active, true)
            .Add(p => p.TargetId, "some-dialog"));

        // No JSException/InvalidOperationException means the exact configured module path
        // ("./js/beeday-dialog-focus.js", no "?v=..." suffix) was the one actually imported.
    }

    private static void SetupDialogFocusModule(BunitContext context)
    {
        var module = context.JSInterop.SetupModule("./js/beeday-dialog-focus.js");
        module.SetupVoid("deactivate", _ => true);
        module.Setup<bool>("activate", _ => true).SetResult(true);
        module.SetupVoid("focusFirstInvalid", _ => true);
    }

    [Fact]
    public void EditorModalShellRendersHardcodedEnglishDefaultsAndWiresCancel()
    {
        using var context = new BunitContext();
        SetupDialogFocusModule(context);
        var cancelled = false;

        var cut = context.Render<EditorModalShell>(parameters => parameters
            .Add(p => p.Model, new object())
            .Add(p => p.Title, "Edit habit")
            .Add(p => p.TitleId, "edit-habit-title")
            .Add(p => p.ShowDelete, true)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => cancelled = true)));

        var dialog = cut.Find("[role='dialog']");
        Assert.Equal("true", dialog.GetAttribute("aria-modal"));
        Assert.Contains("SAVE", cut.Markup);
        Assert.Contains("Delete", cut.Markup);
        Assert.Contains("Cancel", cut.Markup);

        cut.Find(".editor-modal__cancel-action").Click();
        Assert.True(cancelled);
    }

    [Fact]
    public void EditorModalShellHonorsCustomSubmitLabelOverridingTheHardcodedDefault()
    {
        using var context = new BunitContext();
        SetupDialogFocusModule(context);

        var cut = context.Render<EditorModalShell>(parameters => parameters
            .Add(p => p.Model, new object())
            .Add(p => p.Title, "Edit habit")
            .Add(p => p.TitleId, "edit-habit-title")
            .Add(p => p.SubmitLabel, "Update"));

        Assert.Contains("Update", cut.Markup);
        Assert.DoesNotContain("SAVE", cut.Markup);
    }

    [Fact]
    public void SortableRendersOneListItemPerIdWithRoleListAndAriaLabel()
    {
        using var context = new BunitContext();
        var sortableModule = context.JSInterop.SetupModule("./js/beeday-sortable.js");
        sortableModule.SetupVoid("initialize", _ => true);
        sortableModule.SetupVoid("dispose", _ => true);
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        var cut = context.Render<BeeDaySortable>(parameters => parameters
            .Add(p => p.ItemIds, new[] { first, second })
            .Add(p => p.CollectionKey, "habits")
            .Add(p => p.ItemTemplate, (RenderFragment<Guid>)(id => builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddContent(1, id.ToString());
                builder.CloseElement();
            }))
            .Add(p => p.OnReorder, EventCallback.Factory.Create<SortableReorderEvent>(this, _ => { })));

        var list = cut.Find("[role='list']");
        Assert.Equal("habits", list.GetAttribute("data-sortable-key"));
        Assert.Equal(2, cut.FindAll("[role='listitem']").Count);
    }

    [Fact]
    public async Task SortableInvokesOnReorderThroughItsJSInvokableEntryPointWithALocalTestHandler()
    {
        using var context = new BunitContext();
        var sortableModule = context.JSInterop.SetupModule("./js/beeday-sortable.js");
        sortableModule.SetupVoid("initialize", _ => true);
        sortableModule.SetupVoid("dispose", _ => true);
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        SortableReorderEvent? received = null;

        var cut = context.Render<BeeDaySortable>(parameters => parameters
            .Add(p => p.ItemIds, new[] { first, second })
            .Add(p => p.CollectionKey, "habits")
            .Add(p => p.ItemTemplate, (RenderFragment<Guid>)(id => builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddContent(1, id.ToString());
                builder.CloseElement();
            }))
            .Add(p => p.OnReorder, EventCallback.Factory.Create<SortableReorderEvent>(this, evt => received = evt)));

        // Simulates the JS side calling back into .NET after a drag gesture completes — there is
        // no Lab consumer page yet (galleries land in a later Sprint), so this local test handler
        // is what proves the OnReorder wiring itself works end-to-end.
        await cut.Instance.NotifyReorderAsync(first.ToString(), second.ToString(), placeAfter: true);

        Assert.NotNull(received);
        Assert.Equal(first.ToString(), received!.ItemId);
        Assert.Equal(second.ToString(), received.TargetItemId);
        Assert.True(received.PlaceAfter);
    }

    [Fact]
    public void SortableOrderMovesAnItemAfterItsTarget()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var c = Guid.NewGuid();
        var original = new[] { a, b, c };

        var result = SortableOrder.Move(original, a, c, placeAfter: true);

        Assert.Equal([b, c, a], result);
    }

    [Fact]
    public void SortableOrderMovesAnItemBeforeItsTarget()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var c = Guid.NewGuid();
        var original = new[] { a, b, c };

        var result = SortableOrder.Move(original, c, a, placeAfter: false);

        Assert.Equal([c, a, b], result);
    }

    [Fact]
    public void SortableOrderIsANoOpWhenSourceAndTargetAreTheSame()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var original = new[] { a, b };

        var result = SortableOrder.Move(original, a, a, placeAfter: true);

        Assert.Equal(original, result);
    }
}
