namespace BeeDayLab.Web.Components.Pages.Daily.Habits;

/// <summary>
/// COPY (Sprint 33.13, FE33-091) of BeeDay.Web's
/// <c>Components/Features/Habits/HabitVisualState.cs</c> — only the namespace changed. A pure
/// <c>int -&gt; CSS class</c> function with zero Domain/Application coupling: the seven bands are a
/// presentation contract (which colour a habit card/editor wears), not a business rule, so it is
/// copied verbatim rather than adapted. The balance it receives is always
/// <c>PositiveCount - NegativeCount</c> from scenario-seeded data — never recalculated by the Lab.
/// </summary>
public static class HabitVisualState
{
    public static string GetModifier(int balance) => balance switch
    {
        >= 21 => "sky",
        >= 14 => "green",
        >= 7 => "yellow",
        <= -14 => "red-strong",
        <= -7 => "red-medium",
        <= -1 => "red-light",
        _ => "white"
    };

    public static string GetCardClass(int balance) => $"habit-card--{GetModifier(balance)}";
    public static string GetEditorClass(int balance) => $"habit-editor--{GetModifier(balance)}";
}
