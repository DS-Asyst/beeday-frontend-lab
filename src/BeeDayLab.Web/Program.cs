using BeeDayLab.Web.Components;
using BeeDayLab.Web.Components.DesignSystem.Feedback;
using BeeDayLab.Web.Components.Pages.Identity.State;
using BeeDayLab.Web.Localization;
using BeeDayLab.Web.Scenarios;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Sprint 33.8 (FE33-105): ToastService is stateful per-circuit (in-memory Messages list, Changed
// event) — Scoped is the correct lifetime for a Blazor Server circuit, matching how BeeDay.Web
// itself registers it.
builder.Services.AddScoped<ToastService>();

// Sprint 33.10 (FE33-104): ScenarioSelection is the Lab-session equivalent of ToastService — one
// instance per Blazor Server circuit, holding the currently selected ScenarioContext. Scoped for
// the same reason ToastService is.
builder.Services.AddScoped<ScenarioSelection>();

// The illustrative demo provider is stateless/pure (only immutable static sample data) — Singleton
// is correct, unlike ScenarioSelection above. A later Sprint's own feature provider (e.g.
// WalletScenarioProvider) should follow the same Singleton registration for the same reason.
builder.Services.AddSingleton<DemoCardListScenarioProvider>();

// Sprint 33.12 (FE33-077..087): the Identity & Account surface's own scenario providers — every one
// is stateless/pure (a switch over ScenarioContext.State returning static synthetic data), same
// Singleton reasoning as DemoCardListScenarioProvider above.
builder.Services.AddSingleton<ForgotPasswordScenarioProvider>();
builder.Services.AddSingleton<ResendConfirmationScenarioProvider>();
builder.Services.AddSingleton<ResetPasswordScenarioProvider>();
builder.Services.AddSingleton<ConfirmEmailScenarioProvider>();
builder.Services.AddSingleton<ProfileCreationScenarioProvider>();
builder.Services.AddSingleton<AccountScenarioProvider>();

// ProfileCreationState mirrors production's own AddScoped registration: it is per-circuit form/step
// state (Model, Step, IsBusy, ValidationError), the same lifetime reasoning as ToastService/
// ScenarioSelection above.
builder.Services.AddScoped<ProfileCreationState>();

// Sprint 33.10 (FE33-104): a real but minimal request-localization pipeline — CookieRequestCultureProvider
// only. No Accept-Language header sniffing, no query-string provider: culture here is always
// explicit (via the /culture/set cookie below), matching Issue #371's "scenario outputs are
// stable" requirement. This replaces production's AuthenticatedAccountCultureProvider (which reads
// a real, persisted User.Language) with nothing — there is no account concept in the Lab, so an
// unset cookie simply falls back to LabCultures.Default.
builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SetDefaultCulture(LabCultures.Default)
        .AddSupportedCultures(LabCultures.Supported)
        .AddSupportedUICultures(LabCultures.Supported);

    options.RequestCultureProviders =
    [
        new CookieRequestCultureProvider { CookieName = LabCultures.CookieName }
    ];
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();

app.UseRequestLocalization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Sprint 33.10 (FE33-104): generic, small, well-understood ASP.NET Core pattern (not real backend
// infrastructure) — makes the already-extracted, already-tested PublicLanguageSwitcher.razor
// (Sprint 33.9) actually work: it already posts to "/culture/set" with a hidden "returnUrl" field,
// which 404s today because no such route exists in the Lab. Zero changes to that component's file.
app.MapPost("/culture/set", (HttpContext httpContext, [FromForm] string culture, [FromForm] string? returnUrl) =>
{
    if (!LabCultures.Supported.Contains(culture, StringComparer.OrdinalIgnoreCase))
    {
        return Results.BadRequest();
    }

    httpContext.Response.Cookies.Append(
        LabCultures.CookieName,
        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
        LabCultures.CreateCookieOptions(app.Environment.IsDevelopment()));

    var destination = IsLocalPath(returnUrl) ? returnUrl! : "/";
    return Results.LocalRedirect(destination);
});

// Sprint 33.12 (FE33-077): Login.razor posts here (production posts to a real POST /auth/login
// minimal API that sets a real session cookie via BeeDay.Infrastructure identity handlers — EXCLUDE,
// same reasoning as everywhere else in the Lab). This Lab-local endpoint is deterministic and
// synthetic: it sets NO cookie and creates NO session (the Sprint boundary forbids both), and checks
// the submitted credentials against exactly one fixed, obviously-synthetic demo pair
// ("demo@beeday.app" / "BeeDayLab!2026") documented here as the only combination that "succeeds".
// On a match it redirects to the submitted returnUrl (open-redirect-guarded by the same IsLocalPath
// local function /culture/set already uses), falling back to "/profile/create" — there is no real
// authenticated landing page in the Lab yet (a later Sprint's concern) — on no match it redirects to
// "/login?error=invalid", the same query-string-driven feedback branch production's Login.razor
// already renders.
app.MapPost("/auth/login", (HttpContext _, [FromForm] string? email, [FromForm] string? password, [FromForm] string? returnUrl) =>
{
    const string DemoEmail = "demo@beeday.app";
    const string DemoPassword = "BeeDayLab!2026";

    var isDemoCredential =
        string.Equals(email?.Trim(), DemoEmail, StringComparison.OrdinalIgnoreCase) &&
        string.Equals(password, DemoPassword, StringComparison.Ordinal);

    if (!isDemoCredential)
    {
        return Results.LocalRedirect("/login?error=invalid");
    }

    var destination = IsLocalPath(returnUrl) ? returnUrl! : "/profile/create";
    return Results.LocalRedirect(destination);
});

app.Run();

// Deliberately local/minimal rather than porting BeeDay.Web's LoginDestinationResolver.IsLocalPath:
// that type lives under Services/Authentication (EXCLUDE, real auth infrastructure) and this Lab
// has no equivalent concept — the redirect-safety check itself is a generic ASP.NET Core pattern,
// not BeeDay-specific business logic.
static bool IsLocalPath(string? value) =>
    !string.IsNullOrWhiteSpace(value) &&
    value.StartsWith('/') &&
    !value.StartsWith("//", StringComparison.Ordinal) &&
    !value.StartsWith("/\\", StringComparison.Ordinal);
