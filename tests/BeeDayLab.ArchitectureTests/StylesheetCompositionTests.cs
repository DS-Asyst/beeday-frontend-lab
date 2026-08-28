using System.Text.RegularExpressions;
using Xunit;

namespace BeeDayLab.ArchitectureTests;

/// <summary>
/// Sprint 33.18-R (CSS composition remediation): the owner ran the Lab locally and directly
/// observed it was visually substantially different from production. Root-cause audit against
/// <c>DS-Asyst/BeeDay@acce26a</c> found two classes of defect — 6 global stylesheets
/// (<c>activity-design-system.css</c>, <c>settings.css</c>, <c>cards.css</c>, <c>dragdrop.css</c>,
/// <c>identity.css</c>, <c>institutional.css</c>) that were never copied into the Lab at all, and
/// the CSS-isolation bundle (<c>BeeDayLab.Web.styles.css</c>, generated at build from all 44
/// <c>*.razor.css</c> files) that was generated but never linked from <c>App.razor</c> — meaning
/// every component-scoped stylesheet was silently inert since Sprint 33.9. This file is the
/// deterministic regression protection the owner required against that class of defect recurring.
/// It complements, and does not replace, actual owner visual review.
/// </summary>
public sealed class StylesheetCompositionTests
{
    // The exact global stylesheet list App.razor loads in production (DS-Asyst/BeeDay@acce26a),
    // in cascade order, excluding the production-only asset-fingerprinting wrapper (@Assets[...])
    // and font-loading <link> tags, which are infrastructure, not presentation content.
    private static readonly string[] ProductionEquivalentStylesheetsInOrder =
    [
        "app.css",
        "css/variables.css",
        "css/design-system.css",
        "css/activity-design-system.css",
        "css/typography.css",
        "css/editor-modal.css",
        "css/forms.css",
        "css/settings.css",
        "css/cards.css",
        "css/feedback.css",
        "css/dragdrop.css",
        "css/theme.css",
        "css/utilities.css",
        "css/animations.css",
        "css/polish.css",
        "css/wallet.css",
        "css/identity.css",
        "css/institutional.css",
        "css/typography-policy.css",
    ];

    private static readonly Regex StylesheetHref = new("<link[^>]*rel=\"stylesheet\"[^>]*href=\"([^\"]+)\"", RegexOptions.Compiled);

    [Fact]
    public void EveryProductionEquivalentGlobalStylesheetIsLoaded()
    {
        var app = ReadAppRazor();
        var loaded = StylesheetHref.Matches(app).Select(m => m.Groups[1].Value).ToHashSet(StringComparer.Ordinal);

        var missing = ProductionEquivalentStylesheetsInOrder.Where(sheet => !loaded.Contains(sheet)).ToArray();

        Assert.True(
            missing.Length == 0,
            "Stylesheet(s) present in production (acce26a) but not loaded by the Lab's App.razor: "
                + string.Join(", ", missing));
    }

    [Fact]
    public void StylesheetCascadeOrderMatchesProductionRelativeOrder()
    {
        var app = ReadAppRazor();
        var loadedInOrder = StylesheetHref.Matches(app).Select(m => m.Groups[1].Value).ToList();

        // Only compare the relative order of sheets that exist in BOTH apps — Lab-only sheets
        // (emails.css, gallery.css, preview.css: no production equivalent) are filtered out rather
        // than asserted against, since production has no position for them to match.
        var relevantOrder = loadedInOrder.Where(ProductionEquivalentStylesheetsInOrder.Contains).ToArray();

        Assert.Equal(ProductionEquivalentStylesheetsInOrder, relevantOrder);
    }

    [Fact]
    public void TheCssIsolationBundleIsReferenced()
    {
        var app = ReadAppRazor();

        Assert.Contains("BeeDayLab.Web.styles.css", app, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryReferencedGlobalStylesheetFileExistsOnDisk()
    {
        var root = FindRepositoryRoot();
        var cssDirectory = Path.Combine(root, "src", "BeeDayLab.Web", "wwwroot", "css");
        var app = ReadAppRazor();

        var referencedCssFiles = StylesheetHref.Matches(app)
            .Select(m => m.Groups[1].Value)
            .Where(href => href.StartsWith("css/", StringComparison.Ordinal))
            .Select(href => href["css/".Length..]);

        var missing = referencedCssFiles
            .Where(name => !File.Exists(Path.Combine(cssDirectory, name)))
            .ToArray();

        Assert.True(missing.Length == 0, "App.razor references stylesheet(s) missing from wwwroot/css: " + string.Join(", ", missing));
    }

    [Fact]
    public void TheObsoleteBootstrapShellStylesheetIsNeitherLoadedNorPresent()
    {
        var root = FindRepositoryRoot();
        var app = ReadAppRazor();

        Assert.DoesNotContain("lab-shell.css", app, StringComparison.Ordinal);
        Assert.False(
            File.Exists(Path.Combine(root, "src", "BeeDayLab.Web", "wwwroot", "css", "lab-shell.css")),
            "lab-shell.css was a documented Sprint 33.5/33.6 temporary bootstrap placeholder, explicitly superseded by Sprint 33.9's real layout extraction — it must not be reintroduced.");
    }

    [Fact]
    public void EveryComponentScopedStylesheetIsNonEmpty()
    {
        var pagesDirectory = Path.Combine(FindRepositoryRoot(), "src", "BeeDayLab.Web");
        var scopedStylesheets = Directory.EnumerateFiles(pagesDirectory, "*.razor.css", SearchOption.AllDirectories).ToArray();
        Assert.NotEmpty(scopedStylesheets);

        var empty = scopedStylesheets.Where(file => File.ReadAllText(file).Trim().Length == 0).ToArray();

        Assert.True(empty.Length == 0, "Empty *.razor.css file(s) found: " + string.Join(", ", empty.Select(Path.GetFileName)));
    }

    [Fact]
    public void ReconnectModalIsRenderedInTheAppShell()
    {
        // Production always renders <ReconnectModal /> in App.razor's body — its default (Hidden)
        // state renders a closed <dialog>, so this is structurally inert until a real/simulated
        // reconnect state is set, but the composition gap itself (never instantiated anywhere) was
        // part of the root-cause audit and is guarded here so it cannot silently regress again.
        var app = ReadAppRazor();

        Assert.Contains("<ReconnectModal", app, StringComparison.Ordinal);
    }

    private static string ReadAppRazor() =>
        File.ReadAllText(Path.Combine(FindRepositoryRoot(), "src", "BeeDayLab.Web", "Components", "App.razor"));

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BeeDayLab.slnx")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory!.FullName;
    }
}
