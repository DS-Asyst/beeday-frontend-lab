namespace BeeDayLab.Web.Components.DesignSystem.Icons;

public sealed record BeeDayIconDefinition(
    string SymbolId,
    string AssetPath,
    BeeDayIconCategory Category,
    string SemanticName,
    string? DefaultLabel = null,
    BeeDayIconName? Fallback = null);
