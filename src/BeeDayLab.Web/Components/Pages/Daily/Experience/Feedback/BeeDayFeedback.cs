using BeeDayLab.Web.Scenarios;

namespace BeeDayLab.Web.Components.Pages.Daily.Experience.Feedback;

/// <summary>
/// ADAPT (Sprint 33.13, FE33-096) of BeeDay.Web's
/// <c>Components/Features/Experience/Feedback/BeeDayFeedback.cs</c>. The record is a plain immutable
/// payload with no behavior beyond two English summary strings; the single change is retyping
/// <c>ExperienceSource</c> from <c>BeeDay.Domain.Enums.ExperienceSourceType</c> to the Lab-local
/// <see cref="DailyExperienceSource"/>.
///
/// <para><c>ExperienceSummary</c>/<c>HistorySummary</c> are kept for shape parity with production
/// (which documents them as retained-for-compatibility, English-only, and unused by the modal):
/// <c>BeeDayFeedbackModal</c> computes its own culture-aware equivalents through
/// <c>IStringLocalizer&lt;ExperienceResources&gt;</c> instead.</para>
///
/// <para>In the Lab these values are never produced by a real domain event — see
/// <c>LabDashboardState</c>'s documented mock level-up trigger, which constructs this record
/// directly.</para>
/// </summary>
public sealed record BeeDayFeedback(
    Guid EventId,
    Guid ExperienceEntryId,
    int PreviousLevel,
    int NewLevel,
    int LevelsGained,
    long ExperienceAmount,
    DailyExperienceSource ExperienceSource,
    DateTimeOffset OccurredAtUtc)
{
    public string ExperienceSummary => $"+{ExperienceAmount} XP from {FormatSource(ExperienceSource)}";

    public string HistorySummary => $"Reached Level {NewLevel}";

    private static string FormatSource(DailyExperienceSource source) => source switch
    {
        DailyExperienceSource.Habit => "Habit Completed",
        DailyExperienceSource.Task => "Task Completed",
        DailyExperienceSource.Todo => "To-Do Completed",
        DailyExperienceSource.Project => "Project Completed",
        _ => source.ToString(),
    };
}
