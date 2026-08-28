using BeeDayLab.Web.Components.Pages.Daily.Habits.Models;
using BeeDayLab.Web.Scenarios;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;

namespace BeeDayLab.Web.Components.Pages.Daily.Habits.Components;

/// <summary>
/// ADAPT (Sprint 33.13, FE33-091) of BeeDay.Web's
/// <c>Components/Features/Habits/Components/HabitEditorModal.razor(.cs)</c>. The modal itself is
/// presentation-only — <c>EditorModalShell</c> (already in the Lab since Sprint 33.8) plus form
/// fields plus a confirm dialog — so the adaptation is purely a retype of the three
/// <c>BeeDay.Domain.Enums</c> members it renders (<c>HabitDirection</c>/<c>HabitDifficulty</c>/
/// <c>HabitResetCounter</c>) to their Lab-local equivalents.
///
/// <para>The colour band it wears comes from <c>HabitVisualState.GetEditorClass(Model.VisualBalance)</c>
/// — and <c>VisualBalance</c> is seeded from scenario data when an existing habit is opened
/// (<c>DashboardModalState.OpenHabit</c>), never recalculated here.</para>
/// </summary>
public partial class HabitEditorModal
{
    [Inject] private IStringLocalizer<HabitResources> Localizer { get; set; } = default!;

    [Parameter, EditorRequired] public HabitEditorModel Model { get; set; } = new();
    [Parameter] public bool IsEditing { get; set; }
    [Parameter] public EventCallback<HabitEditorModel> OnSave { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback OnDelete { get; set; }
    [Parameter] public string? FallbackFocusSelector { get; set; }

    private bool showDeleteConfirmation;

    private string VisualStateClass => HabitVisualState.GetEditorClass(Model.VisualBalance);
    private bool AllowsPositive => Model.Direction is DailyHabitDirection.Positive or DailyHabitDirection.Both;
    private bool AllowsNegative => Model.Direction is DailyHabitDirection.Negative or DailyHabitDirection.Both;
    private string PositiveAriaPressed => AllowsPositive ? "true" : "false";
    private string NegativeAriaPressed => AllowsNegative ? "true" : "false";

    private string FormatDifficulty(DailyHabitDifficulty difficulty) => difficulty switch
    {
        DailyHabitDifficulty.Trivial => Localizer["DifficultyTrivial"],
        DailyHabitDifficulty.Easy => Localizer["DifficultyEasy"],
        DailyHabitDifficulty.Medium => Localizer["DifficultyMedium"],
        DailyHabitDifficulty.Hard => Localizer["DifficultyHard"],
        _ => difficulty.ToString()
    };

    private string FormatResetCounter(DailyHabitResetCounter resetCounter) => resetCounter switch
    {
        DailyHabitResetCounter.Daily => Localizer["ResetCounterDaily"],
        DailyHabitResetCounter.Weekly => Localizer["ResetCounterWeekly"],
        DailyHabitResetCounter.Monthly => Localizer["ResetCounterMonthly"],
        _ => resetCounter.ToString()
    };

    private void TogglePositive() => Model.Direction = (AllowsPositive, AllowsNegative) switch
    {
        (true, true) => DailyHabitDirection.Negative,
        (true, false) => DailyHabitDirection.Positive,
        _ => DailyHabitDirection.Both
    };

    private void ToggleNegative() => Model.Direction = (AllowsPositive, AllowsNegative) switch
    {
        (true, true) => DailyHabitDirection.Positive,
        (false, true) => DailyHabitDirection.Negative,
        _ => DailyHabitDirection.Both
    };

    private Task Save() => OnSave.InvokeAsync(Model);

    private Task Cancel()
    {
        showDeleteConfirmation = false;
        return OnCancel.InvokeAsync();
    }

    private void RequestDelete() => showDeleteConfirmation = true;

    private void CloseDeleteConfirmation() => showDeleteConfirmation = false;

    private async Task ConfirmDelete()
    {
        showDeleteConfirmation = false;
        await OnDelete.InvokeAsync();
    }

    private Task HandleKeyDown(KeyboardEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Key != "Escape")
        {
            return Task.CompletedTask;
        }

        if (showDeleteConfirmation)
        {
            showDeleteConfirmation = false;
            return Task.CompletedTask;
        }

        return Cancel();
    }
}
