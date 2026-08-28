using System.Globalization;
using BeeDayLab.Web.Components.Pages.Daily.Habits;
using BeeDayLab.Web.Scenarios;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;

namespace BeeDayLab.Web.Components.Pages.Daily.Components;

/// <summary>
/// ADAPT (Sprint 33.13, FE33-090 dependency) of BeeDay.Web's
/// <c>Components/Features/Dashboard/Components/HabitCard.razor(.cs)</c>. The only change is retyping
/// <see cref="Direction"/> from <c>BeeDay.Domain.Enums.HabitDirection</c> to the Lab-local
/// <see cref="DailyHabitDirection"/>; markup, aria contracts and balance formatting are verbatim.
///
/// <para><see cref="Balance"/> is <c>PositiveCount - NegativeCount</c> over two scenario-seeded
/// counters and feeds the copied <c>HabitVisualState</c> band function — presentation formatting,
/// not a reward rule.</para>
/// </summary>
public partial class HabitCard
{
    [Inject] private IStringLocalizer<DashboardResources> Localizer { get; set; } = default!;

    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter] public string Description { get; set; } = string.Empty;
    [Parameter] public string SearchTerm { get; set; } = string.Empty;
    [Parameter] public DailyHabitDirection Direction { get; set; } = DailyHabitDirection.Both;
    [Parameter] public int PositiveCount { get; set; }
    [Parameter] public int NegativeCount { get; set; }
    [Parameter] public bool Featured { get; set; }
    [Parameter] public EventCallback OnPositive { get; set; }
    [Parameter] public EventCallback OnNegative { get; set; }
    [Parameter] public EventCallback OnEdit { get; set; }

    private int Balance => PositiveCount - NegativeCount;

    private string FormattedBalance =>
        Balance > 0 ? $"+{Balance}" : Balance.ToString(CultureInfo.CurrentCulture);

    private bool AllowsPositive => Direction is DailyHabitDirection.Positive or DailyHabitDirection.Both;
    private bool AllowsNegative => Direction is DailyHabitDirection.Negative or DailyHabitDirection.Both;

    private string DirectionText => Direction switch
    {
        DailyHabitDirection.Positive => Localizer["DirectionPositive"],
        DailyHabitDirection.Negative => Localizer["DirectionNegative"],
        _ => Localizer["DirectionBoth"]
    };

    private string EditAriaLabel => Localizer["HabitEditAriaLabel", Title];
    private string RegisterPositiveAriaLabel => Localizer["RegisterPositiveAriaLabel", Title];
    private string RegisterNegativeAriaLabel => Localizer["RegisterNegativeAriaLabel", Title];

    private string CardCssClass => $"habit-card {HabitVisualState.GetCardClass(Balance)}";

    private Task HandleBodyKeyDown(KeyboardEventArgs args) =>
        args.Key is "Enter" or " " ? OnEdit.InvokeAsync() : Task.CompletedTask;
}
