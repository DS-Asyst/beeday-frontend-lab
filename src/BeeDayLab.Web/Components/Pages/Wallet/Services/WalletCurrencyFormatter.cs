using System.Globalization;

namespace BeeDayLab.Web.Components.Pages.Wallet.Services;

/// <summary>
/// Presentation only — the Wallet's currency is USD, a financial/business decision with no
/// per-user configuration surface anywhere in the domain, and this formatter never changes it.
/// What DOES follow the active request culture (set by RequestLocalizationMiddleware onto
/// CultureInfo.CurrentCulture) is number presentation: grouping/decimal separators and symbol
/// placement — e.g. "$1,234.56" under en-US vs "$ 1.234,56" under pt-BR, both still USD. Cloning
/// the current culture's NumberFormat and overriding only CurrencySymbol keeps language/culture of
/// presentation cleanly separate from the currency of the underlying financial data.
/// </summary>
public static class WalletCurrencyFormatter
{
    private const string CurrencySymbol = "$";

    public static string Format(decimal value) => value.ToString("C", BuildFormat(CultureInfo.CurrentCulture));

    public static string FormatSigned(decimal value, bool isIncome)
        => $"{(isIncome ? "+" : "-")}{Format(Math.Abs(value))}";

    private static NumberFormatInfo BuildFormat(CultureInfo culture)
    {
        var format = (NumberFormatInfo)culture.NumberFormat.Clone();
        format.CurrencySymbol = CurrencySymbol;
        return format;
    }
}
