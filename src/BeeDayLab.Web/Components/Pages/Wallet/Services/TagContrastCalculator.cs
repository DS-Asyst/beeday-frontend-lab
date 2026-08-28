namespace BeeDayLab.Web.Components.Pages.Wallet.Services;

public static class TagContrastCalculator
{
    public static string GetTextColor(string? hexColor)
    {
        if (string.IsNullOrWhiteSpace(hexColor))
        {
            return "#ffffff";
        }
        var value = hexColor.Trim().TrimStart('#');
        if (value.Length != 6 || !int.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out var rgb))
        {
            return "#ffffff";
        }

        var r = (rgb >> 16) & 255;
        var g = (rgb >> 8) & 255;
        var b = rgb & 255;
        var luminance = (0.299 * r) + (0.587 * g) + (0.114 * b);
        return luminance > 160 ? "#17111f" : "#ffffff";
    }
}
