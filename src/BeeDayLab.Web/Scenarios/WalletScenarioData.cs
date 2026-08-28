namespace BeeDayLab.Web.Scenarios;

// Sprint 33.14 (FE33-098..100) — the single Lab-local presentation contract for Wallet.
// Production types this replaces live in BeeDay.Application.Features.Wallets.Responses and
// BeeDay.Domain.Enums. The Lab carries only already-resolved display values; it never calculates
// balances, aggregates transactions, or reproduces a financial rule.

public enum WalletTransactionType
{
    Income,
    Expense
}

public sealed record WalletSummaryData(
    Guid WalletId,
    decimal Balance,
    decimal TotalIncome,
    decimal TotalExpenses,
    int TransactionCount,
    DateTimeOffset UpdatedAtUtc);

public sealed record WalletTransactionData(
    Guid Id,
    Guid WalletId,
    string Description,
    decimal Amount,
    decimal SignedAmount,
    WalletTransactionType Type,
    DateOnly TransactionDate,
    Guid? WalletTagId,
    string? WalletTagName,
    string? WalletTagColor,
    string Notes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record WalletTagData(
    Guid Id,
    string Name,
    string Color,
    int TransactionCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record WalletPagedTransactionsData(
    IReadOnlyList<WalletTransactionData> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record WalletScenarioData(
    WalletSummaryData Summary,
    IReadOnlyList<WalletTagData> Tags,
    IReadOnlyList<WalletTransactionData> Transactions);
