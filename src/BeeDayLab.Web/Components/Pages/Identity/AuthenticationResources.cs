namespace BeeDayLab.Web.Components.Pages.Identity;

/// <summary>
/// Marker type for resolving <c>Login.razor</c>'s own resource catalog via
/// <c>IStringLocalizer&lt;AuthenticationResources&gt;</c>. Ported verbatim in shape from
/// BeeDay.Web's <c>Components/Features/Authentication/AuthenticationResources.cs</c>
/// (Sprint 33.12, FE33-077) — only the namespace changed, to mirror this Lab's flat
/// <c>Components/Pages/Identity/</c> placement for the whole Identity &amp; Account surface (see the
/// Sprint 33.12 issue's scope table — production splits Authentication/Identity/ProfileCreation/
/// Onboarding/Account into separate Feature folders; the Lab groups them under one Identity area
/// since this single Sprint owns all of them together).
/// </summary>
public sealed class AuthenticationResources;
