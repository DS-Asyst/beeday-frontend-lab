using Microsoft.AspNetCore.Components;

namespace BeeDayLab.Web.Components.DesignSystem.Layout;

public partial class BeeDayHero
{
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter] public string? Eyebrow { get; set; }
    [Parameter] public string? Subtitle { get; set; }
    [Parameter] public RenderFragment? BrandContext { get; set; }

    /// <summary>
    /// Sprint 29.4: rendered on the opposite end of the same row as <see cref="BrandContext"/>
    /// (top-right on a colored header+hero surface) — the editorial microsite's contextual section
    /// navigation. Optional and independent of BrandContext so other BeeDayHero consumers (Wallet,
    /// onboarding) are unaffected. Kept as a plain RenderFragment parameter — no real auth/routing
    /// state is wired up here; whatever the Lab consumer passes in is the caller's concern.
    /// </summary>
    [Parameter] public RenderFragment? HeaderNav { get; set; }
    [Parameter] public RenderFragment? Illustration { get; set; }
    [Parameter] public RenderFragment? PrimaryAction { get; set; }
    [Parameter] public RenderFragment? SupportingContent { get; set; }
    [Parameter] public BeeDayHeroVariant Variant { get; set; } = BeeDayHeroVariant.Default;

    /// <summary>
    /// The solid COR0-COR9 background this hero renders on, paired automatically with its
    /// WCAG-checked foreground (docs/brand/03-color-palette.md). Null keeps the hero's neutral/
    /// transparent default background for non-solid contexts (e.g. onboarding). Generic here — not
    /// restricted to the two page-header-eligible tokens (Cor0/Cor8) — because BeeDayHero is a
    /// shared primitive with non-page-header consumers too (e.g. the Wallet hero); that narrower
    /// restriction is enforced by the Institutional page templates, not this component.
    /// </summary>
    [Parameter] public BeeDayPaletteToken? Surface { get; set; }

    /// <summary>Roughly halves the hero's vertical padding — the Wallet hero (Sprint 27.11) uses this.</summary>
    [Parameter] public bool Compact { get; set; }

    [Parameter] public string? Class { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass
    {
        get
        {
            var variantClass = Variant switch
            {
                BeeDayHeroVariant.Onboarding => "beeday-hero--onboarding",
                _ => null
            };
            var surfaceClass = Surface?.ToSurfaceCssClass();
            var compactClass = Compact ? "beeday-hero--compact" : null;

            return string.Join(' ', new[] { "beeday-hero", variantClass, surfaceClass, compactClass, Class }
                .Where(value => !string.IsNullOrWhiteSpace(value)));
        }
    }
}
