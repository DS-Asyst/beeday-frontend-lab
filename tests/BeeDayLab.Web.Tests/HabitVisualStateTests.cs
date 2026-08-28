using BeeDayLab.Web.Components.Pages.Daily.Habits;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Sprint 33.13 (FE33-091) tests for <see cref="HabitVisualState"/> — the pure
/// <c>int -&gt; CSS class</c> band function copied verbatim from production. Every one of the seven
/// bands is pinned at BOTH sides of its boundary, so a future edit that shifts a threshold by one
/// fails here rather than silently repainting habit cards.
/// </summary>
public sealed class HabitVisualStateTests
{
    [Theory]
    // sky: >= 21
    [InlineData(21, "sky")]
    [InlineData(100, "sky")]
    // green: >= 14 (and below 21)
    [InlineData(20, "green")]
    [InlineData(14, "green")]
    // yellow: >= 7 (and below 14)
    [InlineData(13, "yellow")]
    [InlineData(7, "yellow")]
    // white: the neutral band, 0..6
    [InlineData(6, "white")]
    [InlineData(0, "white")]
    // red-light: <= -1 (and above -7)
    [InlineData(-1, "red-light")]
    [InlineData(-6, "red-light")]
    // red-medium: <= -7 (and above -14)
    [InlineData(-7, "red-medium")]
    [InlineData(-13, "red-medium")]
    // red-strong: <= -14
    [InlineData(-14, "red-strong")]
    [InlineData(-100, "red-strong")]
    public void GetModifierReturnsTheExpectedBandForEachBoundary(int balance, string expected) =>
        Assert.Equal(expected, HabitVisualState.GetModifier(balance));

    [Fact]
    public void AllSevenBandsAreReachable()
    {
        var bands = Enumerable.Range(-30, 61).Select(HabitVisualState.GetModifier).Distinct().ToList();

        Assert.Equal(7, bands.Count);
    }

    [Theory]
    [InlineData(24, "habit-card--sky")]
    [InlineData(0, "habit-card--white")]
    [InlineData(-18, "habit-card--red-strong")]
    public void GetCardClassPrefixesTheBand(int balance, string expected) =>
        Assert.Equal(expected, HabitVisualState.GetCardClass(balance));

    [Theory]
    [InlineData(24, "habit-editor--sky")]
    [InlineData(0, "habit-editor--white")]
    [InlineData(-18, "habit-editor--red-strong")]
    public void GetEditorClassPrefixesTheBand(int balance, string expected) =>
        Assert.Equal(expected, HabitVisualState.GetEditorClass(balance));
}
