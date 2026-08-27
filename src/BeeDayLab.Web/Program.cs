using BeeDayLab.Web.Components;
using BeeDayLab.Web.Components.DesignSystem.Feedback;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Sprint 33.8 (FE33-105): ToastService is stateful per-circuit (in-memory Messages list, Changed
// event) — Scoped is the correct lifetime for a Blazor Server circuit, matching how BeeDay.Web
// itself registers it.
builder.Services.AddScoped<ToastService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
