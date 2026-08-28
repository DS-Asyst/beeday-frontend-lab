using BeeDayLab.Web.Scenarios;

namespace BeeDayLab.Web.Components.Pages.Daily.Experience.Models;

/// <summary>
/// ADAPT (Sprint 33.13, FE33-096) of BeeDay.Web's
/// <c>Components/Features/Experience/Models/ExperienceViewModel.cs</c>.
///
/// <para><b>Deviation reported by the Sprint brief's own instruction</b> ("if <c>From</c> calls
/// something Domain/Application-typed, stop and reconsider — report this if it happens"): production
/// has TWO <c>From</c> factories. <c>From(UserProfileSummary)</c> is pure presentation arithmetic
/// over four already-resolved numbers and is ported below with its retyped parameter. The second,
/// <c>From(BeeDay.Domain.Experience.UserExperience)</c>, takes a Domain entity directly and is
/// therefore NOT ported at all (EXCLUDE) — porting it would require a Lab-local copy of a Domain
/// type, which ADR-008 forbids outright, and no file in this Sprint's surface calls it: production's
/// only callers of that overload live outside the Daily/Dashboard surface.</para>
///
/// <para><see cref="ProgressPercentage"/> is a clamp/ratio over values the scenario already resolved
/// — display formatting, not the XP curve. The Lab never derives <see cref="Level"/>,
/// <see cref="CurrentExperience"/> or <see cref="RequiredExperience"/> from an experience total.</para>
/// </summary>
public sealed record ExperienceViewModel(
    int Level,
    long CurrentExperience,
    long RequiredExperience,
    long RemainingExperience,
    long TotalExperience = 0)
{
    public double ProgressPercentage => RequiredExperience <= 0
        ? 100d
        : Math.Clamp(CurrentExperience * 100d / RequiredExperience, 0d, 100d);

    public static ExperienceViewModel From(DailyUserProfileSummary profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return new ExperienceViewModel(
            profile.CurrentLevel,
            profile.CurrentLevelExperience,
            profile.ExperienceRequiredForCurrentLevel,
            Math.Max(0, profile.ExperienceRequiredForCurrentLevel - profile.CurrentLevelExperience),
            profile.TotalExperience);
    }
}
