using BeeDayLab.Web.Scenarios;
using Microsoft.AspNetCore.Components;

namespace BeeDayLab.Web.Components.Pages.Daily.Components;

/// <summary>
/// ADAPT (Sprint 33.13, FE33-090 dependency) of BeeDay.Web's
/// <c>Components/Features/Dashboard/Components/ActivityFilterBar.razor(.cs)</c>. The debounced search
/// input and the four-item create menu are ported unchanged — the 300 ms debounce is a presentation
/// concern, not a query optimisation. The only change is retyping <c>ActivityType</c> to the
/// Lab-local <see cref="DailyActivityType"/>.
///
/// <para>The production markup additionally carries an <c>@using BeeDay.Domain.Enums</c> directive
/// that nothing in the file actually resolves against (its <c>ActivityType</c> comes from
/// <c>Components/Features/Common</c>, not from Domain). It is dropped here rather than translated —
/// there is no Lab-local equivalent to point it at, and adding one would imply a dependency the
/// component never had.</para>
/// </summary>
public partial class ActivityFilterBar : IDisposable
{
    private const int SearchDebounceMilliseconds = 300;

    private bool showCreateMenu;
    private string inputValue = string.Empty;
    private CancellationTokenSource? debounceCancellation;

    [Parameter] public string Value { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public EventCallback<DailyActivityType> OnCreate { get; set; }

    protected override void OnParametersSet()
    {
        if (!string.Equals(Value, inputValue, StringComparison.Ordinal))
        {
            inputValue = Value;
        }
    }

    private async Task OnInput(string? value)
    {
        inputValue = value ?? string.Empty;
        debounceCancellation?.Cancel();
        debounceCancellation?.Dispose();
        debounceCancellation = new CancellationTokenSource();

        try
        {
            await Task.Delay(SearchDebounceMilliseconds, debounceCancellation.Token);
            await ValueChanged.InvokeAsync(inputValue);
        }
        catch (OperationCanceledException)
        {
            // A newer input superseded this search.
        }
    }

    private void ToggleCreateMenu() => showCreateMenu = !showCreateMenu;

    private Task CreateHabitAsync() => SelectCreateTypeAsync(DailyActivityType.Habit);
    private Task CreateTaskAsync() => SelectCreateTypeAsync(DailyActivityType.Task);
    private Task CreateTodoAsync() => SelectCreateTypeAsync(DailyActivityType.Todo);
    private Task CreateProjectAsync() => SelectCreateTypeAsync(DailyActivityType.Project);

    private async Task SelectCreateTypeAsync(DailyActivityType type)
    {
        showCreateMenu = false;
        await OnCreate.InvokeAsync(type);
    }

    public void Dispose()
    {
        debounceCancellation?.Cancel();
        debounceCancellation?.Dispose();
    }
}
