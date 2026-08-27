using System.Globalization;

namespace BeeDayLab.Web.Scenarios;

/// <summary>
/// Scoped, per-circuit service (Sprint 33.10, FE33-104) holding the currently selected
/// <see cref="ScenarioContext"/> for the Lab session — same lifetime pattern as
/// <see cref="Components.DesignSystem.Feedback.ToastService"/> from Sprint 33.8 (stateful, one
/// instance per Blazor Server circuit, registered <c>Scoped</c> in Program.cs). Not a UI
/// component: a future scenario-picker gallery page (Sprint 33.16/33.17) binds its controls to
/// <see cref="State"/>/<see cref="Culture"/>/<see cref="Viewport"/> and subscribes to
/// <see cref="Changed"/> to re-render, the same way <c>BeeDayToastHost</c> subscribes to
/// <c>ToastService.Changed</c> today.
/// </summary>
public sealed class ScenarioSelection
{
    private ScenarioState state = ScenarioState.Populated;
    private ViewportPreset? viewport;

    // Mirrors the ambient request culture at the moment this scoped instance is constructed (i.e.
    // whatever the request-localization pipeline in Program.cs already resolved from the
    // LabCultures.CookieName cookie) — an explicit, scenario-driven default, not account-derived.
    private string culture = CultureInfo.CurrentUICulture.Name;

    /// <summary>Raised whenever <see cref="State"/>, <see cref="Culture"/>, or <see cref="Viewport"/> actually changes value.</summary>
    public event Action? Changed;

    public ScenarioState State
    {
        get => state;
        set
        {
            if (state == value)
            {
                return;
            }

            state = value;
            Changed?.Invoke();
        }
    }

    public ViewportPreset? Viewport
    {
        get => viewport;
        set
        {
            if (viewport == value)
            {
                return;
            }

            viewport = value;
            Changed?.Invoke();
        }
    }

    public string Culture
    {
        get => culture;
        set
        {
            if (string.Equals(culture, value, StringComparison.Ordinal))
            {
                return;
            }

            culture = value;
            Changed?.Invoke();
        }
    }

    /// <summary>The current selection as an immutable <see cref="ScenarioContext"/>, ready to hand to any <see cref="IScenarioProvider{TData}"/>.</summary>
    public ScenarioContext Context => new(State, Culture, Viewport);
}
