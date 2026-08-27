namespace BeeDayLab.Web.Localization;

/// <summary>
/// Lab adaptation (Sprint 33.10, FE33-104) of BeeDay.Web's <c>Localization/BeeDayCultures.cs</c>:
/// keeps the Domain-free parts — supported/default culture codes, the culture cookie's name, and
/// the shared <see cref="CreateCookieOptions"/> factory — and drops
/// <c>FromUserLanguage</c>/<c>ToUserLanguage</c> entirely, since both convert to/from
/// <c>BeeDay.Domain.Enums.UserLanguage</c>, a Domain dependency ADR-008 forbids in the Lab. There is
/// no real account/user concept here, so no Lab-local replacement for that enum is fabricated.
///
/// Named <c>LabCultures</c> rather than a straight copy of <c>BeeDayCultures</c> — it is
/// meaningfully smaller — and given its own cookie name (<see cref="CookieName"/>,
/// <c>"BeeDayLab.Culture"</c>, distinct from production's <c>"BeeDay.Culture"</c>) so a browser
/// with both apps open locally never confuses the two.
///
/// Production's other half, <c>AuthenticatedAccountCultureProvider</c> (resolves culture from a
/// real, persisted <c>User.Language</c> for a session with no explicit cookie yet), is EXCLUDE —
/// not ported at all, not even adapted: it is 100% real backend infrastructure (reads a real
/// <c>User</c> entity handed off from real authentication middleware). Its Lab replacement is
/// explicit, scenario-driven culture selection via <see cref="Scenarios.ScenarioSelection"/>
/// instead of account-derived culture.
/// </summary>
public static class LabCultures
{
    public const string English = "en-US";
    public const string Portuguese = "pt-BR";
    public const string Default = English;

    /// <summary>
    /// Name of the cookie that persists the effective UI culture for the Lab. Deliberately
    /// distinct from production's <c>"BeeDay.Culture"</c> cookie name.
    /// </summary>
    public const string CookieName = "BeeDayLab.Culture";

    public static readonly string[] Supported = [English, Portuguese];

    /// <summary>
    /// Attributes shared by every place that writes the culture cookie — kept in one place so they
    /// can never drift apart, mirroring the same factory in production's <c>BeeDayCultures</c>.
    /// </summary>
    public static CookieOptions CreateCookieOptions(bool isDevelopment) => new()
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Lax,
        Secure = !isDevelopment,
        Expires = DateTimeOffset.UtcNow.AddYears(1)
    };
}
