using System.Text.RegularExpressions;
using Xunit;

namespace BeeDayLab.ArchitectureTests;

/// <summary>
/// Sprint 33.10 (FE33-104, Issue #371) architecture guards, in the same source-text-scanning style
/// as <see cref="LabBoundaryTests"/>: proves the scenario engine under
/// <c>BeeDayLab.Web.Scenarios</c> has no service/database coupling (Issue #371's acceptance
/// criterion, enforced in code, not just by convention), and that the Lab's culture catalog
/// (<c>LabCultures.cs</c>) carries no Domain dependency — the reason its production counterpart's
/// two conversion methods (<c>FromUserLanguage</c>/<c>ToUserLanguage</c>) were deliberately dropped
/// rather than ported.
/// </summary>
public sealed class ScenarioAndLocalizationBoundaryTests
{
    private static readonly string[] ForbiddenSubstrings =
    [
        "BeeDay.Domain",
        "BeeDay.Application",
        "BeeDay.Infrastructure",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.Data.SqlClient",
        "System.Data.SqlClient",
        "ISender",
        "BeeDayWebService",
        "ConnectionString",
        "HttpClient",
        "System.Net.Http",
    ];

    [Fact]
    public void NoScenarioEngineFileReferencesAServiceOrDatabase()
    {
        var scenariosDirectory = Path.Combine(FindRepositoryRoot(), "src", "BeeDayLab.Web", "Scenarios");
        Assert.True(Directory.Exists(scenariosDirectory), $"Expected directory not found: {scenariosDirectory}");

        var sourceFiles = Directory.EnumerateFiles(scenariosDirectory, "*.cs", SearchOption.AllDirectories).ToList();
        Assert.NotEmpty(sourceFiles);

        var violations = new List<string>();

        foreach (var file in sourceFiles)
        {
            var content = StripComments(File.ReadAllText(file));

            foreach (var forbidden in ForbiddenSubstrings)
            {
                if (content.Contains(forbidden, StringComparison.Ordinal))
                {
                    violations.Add($"{Path.GetFileName(file)} references '{forbidden}'");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "Scenario engine boundary violated (Issue #371 acceptance criterion):" + Environment.NewLine
                + string.Join(Environment.NewLine, violations));
    }

    /// <summary>
    /// Sprint 33.12 (Issue #373) guard: proves none of this Sprint's new Identity &amp; Account
    /// surface files (Login/CreateProfile/Tutorial/Account/etc. and their scenario providers) leak a
    /// real Domain/Application/Infrastructure dependency, and — the boundary specific to this
    /// Sprint's brief — that <c>DomainErrorLocalizer</c> is never called from any of them, since it
    /// is explicitly excluded (never ported, same treatment <c>ValidationMessageLocalizer.cs</c> got
    /// in Sprint 33.8) rather than adapted. A failure page instead carries a synthetic outcome
    /// straight from a scenario provider or SharedResources' own already-real "DomainErrorGeneric"
    /// string — never a translated real exception.
    /// </summary>
    [Fact]
    public void NoIdentityAccountSurfaceFileReferencesTheProductionBackendOrTheExcludedDomainErrorLocalizer()
    {
        var identityDirectory = Path.Combine(FindRepositoryRoot(), "src", "BeeDayLab.Web", "Components", "Pages", "Identity");
        Assert.True(Directory.Exists(identityDirectory), $"Expected directory not found: {identityDirectory}");

        var sourceFiles = Directory.EnumerateFiles(identityDirectory, "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(identityDirectory, "*.razor", SearchOption.AllDirectories))
            .ToList();
        Assert.NotEmpty(sourceFiles);

        var forbidden = ForbiddenSubstrings.Append("DomainErrorLocalizer").ToArray();
        var violations = new List<string>();

        foreach (var file in sourceFiles)
        {
            var content = StripComments(File.ReadAllText(file));

            foreach (var term in forbidden)
            {
                if (content.Contains(term, StringComparison.Ordinal))
                {
                    violations.Add($"{Path.GetFileName(file)} references '{term}'");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "Identity & Account surface boundary violated (Sprint 33.12 brief):" + Environment.NewLine
                + string.Join(Environment.NewLine, violations));
    }

    /// <summary>
    /// Sprint 33.13 (Issue #374) guard, and the widest one in this file: proves that nothing in the
    /// whole Daily/productivity surface — the two pages, the five dashboard components, the four
    /// editor dialogs, the project workspace, the experience bar, the ported feedback store/modal, all
    /// four editor models, <c>HabitVisualState</c>, <c>DashboardModalState</c> and
    /// <c>LabDashboardState</c> — references a real Domain/Application/Infrastructure type, a MediatR
    /// pipeline, or the production service facade.
    ///
    /// <para>The MediatR terms matter specifically here: production feeds its level-up modal from
    /// <c>BeeDayFeedbackEventHandler : INotificationHandler&lt;DomainEventNotification&gt;</c>, which
    /// this Sprint deliberately did NOT port (<c>LabDashboardState</c> calls
    /// <c>BeeDayFeedbackStore.Add(...)</c> directly with synthetic data instead). This test is what
    /// stops a future Sprint quietly reintroducing that pipeline to "make the modal real".</para>
    /// </summary>
    [Fact]
    public void NoDailySurfaceFileReferencesTheProductionBackendOrAnyDomainEventPipeline()
    {
        var dailyDirectory = Path.Combine(
            FindRepositoryRoot(), "src", "BeeDayLab.Web", "Components", "Pages", "Daily");
        Assert.True(Directory.Exists(dailyDirectory), $"Expected directory not found: {dailyDirectory}");

        var sourceFiles = Directory.EnumerateFiles(dailyDirectory, "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(dailyDirectory, "*.razor", SearchOption.AllDirectories))
            .ToList();
        Assert.NotEmpty(sourceFiles);

        var forbidden = ForbiddenSubstrings
            .Concat([
                "MediatR",
                "INotificationHandler",
                "DomainEventNotification",
                "UserLeveledUpDomainEvent",
                "BeeDayFeedbackEventHandler",
                "AuthenticatedUserInitializer",
                "InvalidDomainStateException",
            ])
            .ToArray();

        var violations = new List<string>();

        foreach (var file in sourceFiles)
        {
            var content = StripComments(File.ReadAllText(file));

            foreach (var term in forbidden)
            {
                if (content.Contains(term, StringComparison.Ordinal))
                {
                    violations.Add($"{Path.GetFileName(file)} references '{term}'");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "Daily surface boundary violated (Sprint 33.13, Issue #374):" + Environment.NewLine
                + string.Join(Environment.NewLine, violations));
    }

    /// <summary>
    /// Sprint 33.13 (Issue #374): the Daily surface's Lab-local presentation contract must be exactly
    /// one translation layer living in <c>Scenarios/</c> — not per-page reinventions. Every
    /// <c>Daily*</c> record/enum this Sprint introduced is therefore required to be declared under
    /// <c>Scenarios/</c>, and no file under <c>Components/Pages/Daily/</c> may declare its own
    /// stand-in for a Domain enum or an Application response DTO.
    /// </summary>
    [Fact]
    public void TheDailyPresentationContractIsDeclaredOnlyOnceUnderScenarios()
    {
        var root = FindRepositoryRoot();
        var scenariosDirectory = Path.Combine(root, "src", "BeeDayLab.Web", "Scenarios");
        var dailyDirectory = Path.Combine(root, "src", "BeeDayLab.Web", "Components", "Pages", "Daily");

        var expectedTypes = new[]
        {
            "DailyActivityType", "DailyTaskRepeat", "DailyProjectStatus", "DailyExperienceSource",
            "DailyActivityAttribute", "DailyHabitDirection", "DailyHabitDifficulty", "DailyHabitResetCounter",
            "DailyUserProfileSummary", "DailyHabitSummary", "DailyTaskSummary", "DailyTodoSummary",
            "DailyProjectSummary", "DailyDashboardScenarioData",
        };

        var scenarioSource = string.Concat(
            Directory.EnumerateFiles(scenariosDirectory, "*.cs", SearchOption.AllDirectories)
                .Select(File.ReadAllText));

        foreach (var type in expectedTypes)
        {
            Assert.True(
                Regex.IsMatch(scenarioSource, $@"\b(record|enum)\s+{type}\b"),
                $"'{type}' must be declared under Scenarios/ — it is the Sprint's single translation layer.");
        }

        // ...and nothing under Components/Pages/Daily/ redeclares any of them, or invents its own
        // copy of the Domain enums / Application DTOs they replace.
        var forbiddenDeclarations = expectedTypes
            .Concat(["ActivityType", "TaskRepeat", "ProjectStatus", "ExperienceSourceType",
                     "ActivityAttribute", "HabitDirection", "HabitDifficulty", "HabitResetCounter",
                     "UserProfileSummary", "HabitSummary", "TaskSummary", "TodoSummary",
                     "ProjectSummary", "DashboardResponse"])
            .ToArray();

        var violations = new List<string>();

        foreach (var file in Directory.EnumerateFiles(dailyDirectory, "*.cs", SearchOption.AllDirectories))
        {
            var content = StripComments(File.ReadAllText(file));

            foreach (var type in forbiddenDeclarations)
            {
                if (Regex.IsMatch(content, $@"\b(record|enum|class)\s+{type}\b"))
                {
                    violations.Add($"{Path.GetFileName(file)} declares its own '{type}'");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "The Daily presentation contract must not be reinvented per page:" + Environment.NewLine
                + string.Join(Environment.NewLine, violations));
    }

    /// <summary>
    /// Sprint 33.13 (Issue #374 item 5, ADR-008 "não recriar cálculo de regra de negócio (XP...)"):
    /// the Lab must never reproduce the real XP/leveling rule. The one XP number in the whole surface
    /// is <c>DailyDashboardScenarioData.XpGainPerAction</c>, a fixed scenario-supplied constant, and
    /// the only thing done with it is addition into a running display total.
    /// </summary>
    [Fact]
    public void NoDailySurfaceFileReproducesTheExperienceOrLevelingCalculation()
    {
        var dailyDirectory = Path.Combine(
            FindRepositoryRoot(), "src", "BeeDayLab.Web", "Components", "Pages", "Daily");

        // Names production's own reward/leveling machinery carries. None of it may appear in the Lab.
        var forbidden = new[]
        {
            "ExperienceCalculator",
            "LevelCurve",
            "ExperienceForNextLevel",
            "CalculateLevel",
            "UserExperience",
            "RewardEngine",
            "LevelUpData",
        };

        var violations = new List<string>();

        foreach (var file in Directory.EnumerateFiles(dailyDirectory, "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(dailyDirectory, "*.razor", SearchOption.AllDirectories)))
        {
            var content = StripComments(File.ReadAllText(file));

            foreach (var term in forbidden)
            {
                if (content.Contains(term, StringComparison.Ordinal))
                {
                    violations.Add($"{Path.GetFileName(file)} references '{term}'");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "The Lab must never recreate the XP/leveling rule (ADR-008):" + Environment.NewLine
                + string.Join(Environment.NewLine, violations));
    }

    /// <summary>
    /// Sprint 33.14 (Issue #375): every Wallet page/component/model/service/state file is isolated
    /// from the production runtime. The only replacement types are the centralized
    /// <c>Wallet*</c> records/enums under <c>Scenarios/</c>.
    /// </summary>
    [Fact]
    public void NoWalletSurfaceFileReferencesTheProductionBackendOrPersistence()
    {
        var walletDirectory = Path.Combine(
            FindRepositoryRoot(), "src", "BeeDayLab.Web", "Components", "Pages", "Wallet");
        Assert.True(Directory.Exists(walletDirectory), $"Expected directory not found: {walletDirectory}");

        var sourceFiles = Directory.EnumerateFiles(walletDirectory, "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(walletDirectory, "*.razor", SearchOption.AllDirectories))
            .ToList();
        Assert.NotEmpty(sourceFiles);

        var forbidden = ForbiddenSubstrings
            .Concat([
                "MediatR",
                "EnsureCurrentWalletCommand",
                "GetWalletSummaryQuery",
                "GetWalletTagsQuery",
                "GetTransactionsQuery",
                "CreateTransactionCommand",
                "UpdateTransactionCommand",
                "DeleteTransactionCommand",
                "CreateWalletTagCommand",
                "UpdateWalletTagCommand",
                "DeleteWalletTagCommand",
                "AuthenticatedUserInitializer",
                "SaveTransactionRequest",
                "SaveWalletTagRequest",
            ])
            .ToArray();

        var violations = new List<string>();
        foreach (var file in sourceFiles)
        {
            var content = StripComments(File.ReadAllText(file));
            foreach (var term in forbidden)
            {
                if (content.Contains(term, StringComparison.Ordinal))
                {
                    violations.Add($"{Path.GetFileName(file)} references '{term}'");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "Wallet surface boundary violated (Sprint 33.14, Issue #375):" + Environment.NewLine
                + string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void TheWalletPresentationContractIsDeclaredOnlyOnceUnderScenarios()
    {
        var root = FindRepositoryRoot();
        var scenariosDirectory = Path.Combine(root, "src", "BeeDayLab.Web", "Scenarios");
        var walletDirectory = Path.Combine(root, "src", "BeeDayLab.Web", "Components", "Pages", "Wallet");
        var expectedTypes = new[]
        {
            "WalletTransactionType", "WalletSummaryData", "WalletTransactionData", "WalletTagData",
            "WalletPagedTransactionsData", "WalletScenarioData",
        };

        var scenarioSource = string.Concat(
            Directory.EnumerateFiles(scenariosDirectory, "*.cs", SearchOption.AllDirectories)
                .Select(File.ReadAllText));

        foreach (var type in expectedTypes)
        {
            Assert.True(
                Regex.IsMatch(scenarioSource, $@"\b(record|enum)\s+{type}\b"),
                $"'{type}' must be declared under Scenarios/ as the single Wallet translation layer.");
        }

        var productionTypes = new[]
        {
            "TransactionType", "WalletSummaryResponse", "TransactionResponse", "WalletTagResponse",
            "PagedTransactionsResponse",
        };
        var violations = new List<string>();

        foreach (var file in Directory.EnumerateFiles(walletDirectory, "*.cs", SearchOption.AllDirectories))
        {
            var content = StripComments(File.ReadAllText(file));
            foreach (var type in expectedTypes.Concat(productionTypes))
            {
                if (Regex.IsMatch(content, $@"\b(record|enum|class)\s+{type}\b"))
                {
                    violations.Add($"{Path.GetFileName(file)} declares its own '{type}'");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "The Wallet presentation contract must not be reinvented per component:" + Environment.NewLine
                + string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void WalletDoesNotReproduceBalanceOrFinancialAggregationRules()
    {
        var root = FindRepositoryRoot();
        var files = Directory.EnumerateFiles(
                Path.Combine(root, "src", "BeeDayLab.Web", "Components", "Pages", "Wallet"),
                "*.*",
                SearchOption.AllDirectories)
            .Where(file => file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                || file.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
            .Append(Path.Combine(root, "src", "BeeDayLab.Web", "Scenarios", "WalletScenarioProvider.cs"));

        var forbiddenPatterns = new[]
        {
            @"\.Sum\s*\(",
            @"\.Aggregate\s*\(",
            @"Calculate(Balance|Income|Expenses)",
            @"Recalculate(Balance|Income|Expenses)",
            @"BalanceCalculator",
            @"FinancialRule",
        };

        var violations = new List<string>();
        foreach (var file in files)
        {
            var content = StripComments(File.ReadAllText(file));
            foreach (var pattern in forbiddenPatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
                {
                    violations.Add($"{Path.GetFileName(file)} matches forbidden financial rule '{pattern}'");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "Wallet display values must stay scenario-resolved; no financial aggregation is allowed:" + Environment.NewLine
                + string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void WalletAndItsCanonicalTransitiveStyleSheetsAreLoaded()
    {
        var root = FindRepositoryRoot();
        var cssDirectory = Path.Combine(root, "src", "BeeDayLab.Web", "wwwroot", "css");
        var app = File.ReadAllText(Path.Combine(root, "src", "BeeDayLab.Web", "Components", "App.razor"));
        var required = new[] { "design-system.css", "editor-modal.css", "forms.css", "feedback.css", "wallet.css" };

        foreach (var name in required)
        {
            Assert.True(File.Exists(Path.Combine(cssDirectory, name)), $"Required copied stylesheet is missing: {name}");
            Assert.Single(Regex.Matches(app, $@"css/{Regex.Escape(name)}").Cast<Match>());
        }
    }

    [Fact]
    public void ScenarioEngineUsesOnlyDeterministicPrimitivesNoRandomOrWallClock()
    {
        var scenariosDirectory = Path.Combine(FindRepositoryRoot(), "src", "BeeDayLab.Web", "Scenarios");
        var forbiddenDeterminismSubstrings = new[]
        {
            "new Random(",
            "Guid.NewGuid()",
            "DateTime.Now",
            "DateTimeOffset.UtcNow",
            "DateTime.UtcNow",
        };

        var violations = new List<string>();

        foreach (var file in Directory.EnumerateFiles(scenariosDirectory, "*.cs", SearchOption.AllDirectories))
        {
            var content = StripComments(File.ReadAllText(file));

            foreach (var forbidden in forbiddenDeterminismSubstrings)
            {
                if (content.Contains(forbidden, StringComparison.Ordinal))
                {
                    violations.Add($"{Path.GetFileName(file)} uses non-deterministic '{forbidden}'");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "Scenario providers must be pure functions of ScenarioContext:" + Environment.NewLine
                + string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void LabCulturesHasNoDomainDependency()
    {
        var labCulturesPath = Path.Combine(
            FindRepositoryRoot(), "src", "BeeDayLab.Web", "Localization", "LabCultures.cs");
        Assert.True(File.Exists(labCulturesPath), $"Expected file not found: {labCulturesPath}");

        // Only the code itself must be Domain-free — its XML doc comments legitimately name
        // BeeDay.Domain.Enums.UserLanguage/FromUserLanguage/ToUserLanguage to explain why those
        // conversion methods were deliberately dropped rather than ported, so comments are
        // stripped before asserting.
        var content = StripComments(File.ReadAllText(labCulturesPath));

        Assert.DoesNotContain("BeeDay.Domain", content, StringComparison.Ordinal);
        Assert.DoesNotContain("UserLanguage", content, StringComparison.Ordinal);
        Assert.DoesNotContain("FromUserLanguage", content, StringComparison.Ordinal);
        Assert.DoesNotContain("ToUserLanguage", content, StringComparison.Ordinal);
    }

    [Fact]
    public void NoAuthenticatedAccountCultureProviderIsPortedIntoTheLab()
    {
        // AuthenticatedAccountCultureProvider is 100% real backend infrastructure (reads a real
        // User entity handed off from real authentication middleware) — EXCLUDE, full stop. This
        // guards against a future Sprint accidentally reintroducing it. Comments legitimately name
        // it (explaining the exclusion), so they are stripped before asserting on actual code.
        var localizationDirectory = Path.Combine(FindRepositoryRoot(), "src", "BeeDayLab.Web", "Localization");
        Assert.True(Directory.Exists(localizationDirectory));

        foreach (var file in Directory.EnumerateFiles(localizationDirectory, "*.cs", SearchOption.AllDirectories))
        {
            Assert.DoesNotContain(
                "AuthenticatedAccountCultureProvider",
                StripComments(File.ReadAllText(file)),
                StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Strips <c>//</c>/<c>///</c> line comments, <c>/* */</c> block comments, and (Sprint 33.12
    /// addition, needed once this file started scanning .razor files too) <c>@* *@</c> Razor
    /// comments, so architecture assertions target real code, not explanatory documentation — this
    /// Sprint's own adaptation-note comments legitimately name every excluded dependency they explain
    /// (e.g. "DomainErrorLocalizer.Translate(ex, SharedLocalizer) is replaced with...").
    /// </summary>
    private static string StripComments(string content)
    {
        content = Regex.Replace(content, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
        content = Regex.Replace(content, @"@\*.*?\*@", string.Empty, RegexOptions.Singleline);
        content = Regex.Replace(content, @"//.*$", string.Empty, RegexOptions.Multiline);
        return content;
    }

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
