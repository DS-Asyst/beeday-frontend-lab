namespace BeeDayLab.Web.Scenarios;

/// <summary>
/// Deterministic, presentation-only Wallet dataset. Summary values are explicit scenario values,
/// never sums derived from the transaction collection.
/// </summary>
public sealed class WalletScenarioProvider : IScenarioProvider<WalletScenarioData>
{
    private static readonly DateTimeOffset ReferenceInstant = new(2026, 8, 27, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid WalletId = CreateId(1);
    private static readonly IReadOnlyList<WalletTagData> PopulatedTags = CreateTags();
    private static readonly IReadOnlyList<WalletTransactionData> PopulatedTransactions = CreatePopulatedTransactions();
    private static readonly IReadOnlyList<WalletTransactionData> LargeTransactions = CreateLargeTransactions();

    public WalletScenarioData GetScenario(ScenarioContext context) => context.State switch
    {
        ScenarioState.Empty => Empty,
        ScenarioState.LargeContent => Large,
        ScenarioState.Populated or ScenarioState.NoResults or ScenarioState.Disabled or ScenarioState.Selected => Populated,
        ScenarioState.Loading or ScenarioState.Error => Empty,
        _ => Empty,
    };

    private static WalletScenarioData Empty { get; } = new(
        new WalletSummaryData(WalletId, 0m, 0m, 0m, 0, ReferenceInstant),
        [],
        []);

    private static WalletScenarioData Populated { get; } = new(
        // These values are intentionally pre-resolved and are not derived from the list below.
        new WalletSummaryData(WalletId, 4_285.40m, 8_950.00m, 4_664.60m, 12, ReferenceInstant),
        PopulatedTags,
        PopulatedTransactions);

    private static WalletScenarioData Large { get; } = new(
        // Same rule: explicit display values, not a transaction aggregation.
        new WalletSummaryData(WalletId, 18_420.75m, 31_840.00m, 13_419.25m, 45, ReferenceInstant),
        PopulatedTags,
        LargeTransactions);

    private static IReadOnlyList<WalletTagData> CreateTags() =>
    [
        Tag(100, "Home", "#5247F9", 4),
        Tag(101, "Food", "#FFB100", 3),
        Tag(102, "Transport", "#1CB0F6", 2),
        Tag(103, "Learning", "#58CC02", 2),
        Tag(104, "Leisure", "#CE82FF", 1),
    ];

    private static IReadOnlyList<WalletTransactionData> CreatePopulatedTransactions() =>
    [
        Transaction(200, "Monthly salary", 6_800m, WalletTransactionType.Income, 27, null, "", 0),
        Transaction(201, "Apartment rent", 1_950m, WalletTransactionType.Expense, 26, 100, "Paid by bank transfer", 1),
        Transaction(202, "Grocery market", 286.45m, WalletTransactionType.Expense, 25, 101, "Weekly groceries", 2),
        Transaction(203, "Online course refund", 320m, WalletTransactionType.Income, 24, 103, "", 3),
        Transaction(204, "Subway card", 95m, WalletTransactionType.Expense, 23, 102, "Monthly pass", 4),
        Transaction(205, "Books", 148.90m, WalletTransactionType.Expense, 21, 103, "Design and engineering", 5),
        Transaction(206, "Dinner with friends", 132.70m, WalletTransactionType.Expense, 19, 104, "", 6),
        Transaction(207, "Electricity bill", 214.35m, WalletTransactionType.Expense, 18, 100, "", 7),
        Transaction(208, "Freelance project", 1_830m, WalletTransactionType.Income, 16, null, "Visual consultation", 8),
        Transaction(209, "Coffee shop", 28.50m, WalletTransactionType.Expense, 14, 101, "", 9),
        Transaction(210, "Ride share", 42.80m, WalletTransactionType.Expense, 12, 102, "", 10),
        Transaction(211, "Home supplies", 166.90m, WalletTransactionType.Expense, 10, 100, "", 11),
    ];

    private static IReadOnlyList<WalletTransactionData> CreateLargeTransactions()
    {
        var transactions = new List<WalletTransactionData>(45);
        for (var index = 0; index < 45; index++)
        {
            var type = index % 6 == 0 ? WalletTransactionType.Income : WalletTransactionType.Expense;
            var tag = PopulatedTags[index % PopulatedTags.Count];
            transactions.Add(new WalletTransactionData(
                CreateId(300 + index),
                WalletId,
                $"Synthetic transaction {index + 1:00}",
                25m + (index * 17.35m),
                type == WalletTransactionType.Income ? 25m + (index * 17.35m) : -(25m + (index * 17.35m)),
                type,
                new DateOnly(2026, 8, 27).AddDays(-index),
                tag.Id,
                tag.Name,
                tag.Color,
                index % 3 == 0 ? "Representative large-content note" : string.Empty,
                ReferenceInstant.AddDays(-index),
                ReferenceInstant.AddDays(-index)));
        }

        return transactions;
    }

    private static WalletTagData Tag(int seed, string name, string color, int count) =>
        new(CreateId(seed), name, color, count, ReferenceInstant.AddDays(-seed), ReferenceInstant);

    private static WalletTransactionData Transaction(
        int seed,
        string description,
        decimal amount,
        WalletTransactionType type,
        int day,
        int? tagSeed,
        string notes,
        int offset)
    {
        var tag = tagSeed is null ? null : PopulatedTags.Single(item => item.Id == CreateId(tagSeed.Value));
        return new WalletTransactionData(
            CreateId(seed),
            WalletId,
            description,
            amount,
            type == WalletTransactionType.Income ? amount : -amount,
            type,
            new DateOnly(2026, 8, day),
            tag?.Id,
            tag?.Name,
            tag?.Color,
            notes,
            ReferenceInstant.AddDays(-offset),
            ReferenceInstant.AddDays(-offset));
    }

    private static Guid CreateId(int seed) =>
        new(seed, 0x3314, 0x5A14, 0xBE, 0xED, 0xA1, 0x14, 0x00, 0x00, 0x00, 0x01);
}
