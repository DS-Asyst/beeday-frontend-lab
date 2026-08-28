using Microsoft.AspNetCore.Components;

namespace BeeDayLab.Web.Components.DesignSystem.Icons;

public partial class BeeDayIcon
{
    [Parameter, EditorRequired] public BeeDayIconName Name { get; set; }
    [Parameter] public BeeDayIconSize Size { get; set; } = BeeDayIconSize.Medium;
    [Parameter] public BeeDayIconColor Color { get; set; } = BeeDayIconColor.Current;
    [Parameter] public bool Decorative { get; set; } = true;
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private BeeDayIconDefinition Definition => BeeDayIconRegistry.Resolve(Name);
    private string SpriteReference => $"{BeeDayIconRegistry.SpritePath}#{Definition.SymbolId}";
    private int PixelSize => Size switch
    {
        BeeDayIconSize.ExtraSmall => 12,
        BeeDayIconSize.Small => 16,
        BeeDayIconSize.Medium => 20,
        BeeDayIconSize.Large => 24,
        BeeDayIconSize.ExtraLarge => 32,
        _ => 20
    };

    private string CssClasses => string.Join(' ', new[]
    {
        "beeday-icon",
        $"beeday-icon--{Definition.SymbolId}",
        $"beeday-icon--size-{Size.ToString().ToLowerInvariant()}",
        $"beeday-icon--color-{Color.ToString().ToLowerInvariant()}",
        Class
    }.Where(value => !string.IsNullOrWhiteSpace(value)));

    protected override void OnParametersSet()
    {
        if (!Decorative && string.IsNullOrWhiteSpace(Label))
        {
            throw new InvalidOperationException(
                $"{nameof(BeeDayIcon)} requires a non-empty {nameof(Label)} when {nameof(Decorative)} is false.");
        }
    }
}
