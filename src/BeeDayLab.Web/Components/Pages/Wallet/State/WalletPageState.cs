namespace BeeDayLab.Web.Components.Pages.Wallet.State;

public sealed class WalletPageState
{
    public string Search { get; set; } = string.Empty;
    public string TypeFilter { get; set; } = string.Empty;
    public string TagFilter { get; set; } = string.Empty;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string Sort { get; set; } = "date-desc";
    public int Page { get; set; } = 1;

    public int ActiveFilterCount =>
        (string.IsNullOrWhiteSpace(Search) ? 0 : 1) +
        (string.IsNullOrWhiteSpace(TypeFilter) ? 0 : 1) +
        (string.IsNullOrWhiteSpace(TagFilter) ? 0 : 1) +
        (StartDate.HasValue ? 1 : 0) +
        (EndDate.HasValue ? 1 : 0);

    public bool HasFilters => ActiveFilterCount > 0;

    public void ResetPage() => Page = 1;

    public void ClearFilters()
    {
        Search = string.Empty;
        TypeFilter = string.Empty;
        TagFilter = string.Empty;
        StartDate = null;
        EndDate = null;
        Page = 1;
    }
}
