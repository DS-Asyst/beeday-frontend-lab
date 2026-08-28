using System.Globalization;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Sets <see cref="CultureInfo.CurrentCulture"/>/<see cref="CultureInfo.CurrentUICulture"/> for the
/// duration of one test and restores it afterward — shared by every Sprint 33.11 test that asserts
/// a specific localized string, so results are deterministic regardless of the host machine/CI
/// runner's own default OS culture (mirrors <c>LocalizationResourceTests.CultureScope</c>, which
/// predates this shared version and stays private/unchanged per this Sprint's scope boundary).
/// </summary>
internal sealed class TestCultureScope : IDisposable
{
    private readonly CultureInfo originalCulture;
    private readonly CultureInfo originalUiCulture;

    public TestCultureScope(string culture = "en-US")
    {
        originalCulture = CultureInfo.CurrentCulture;
        originalUiCulture = CultureInfo.CurrentUICulture;

        var target = new CultureInfo(culture);
        CultureInfo.CurrentCulture = target;
        CultureInfo.CurrentUICulture = target;
    }

    public void Dispose()
    {
        CultureInfo.CurrentCulture = originalCulture;
        CultureInfo.CurrentUICulture = originalUiCulture;
    }
}
