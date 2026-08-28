using System.ComponentModel.DataAnnotations;
using BeeDayLab.Web.Scenarios;

namespace BeeDayLab.Web.Components.Pages.Wallet.Models;

public sealed class TransactionFormModel
{
    [Required, StringLength(120, MinimumLength = 2)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999999", ParseLimitsInInvariantCulture = true)]
    public decimal Amount { get; set; }

    public WalletTransactionType Type { get; set; } = WalletTransactionType.Expense;

    [Required]
    public DateTime TransactionDate { get; set; } = new(2026, 8, 27);

    public Guid? WalletTagId { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
