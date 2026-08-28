using System.ComponentModel.DataAnnotations;

namespace BeeDayLab.Web.Components.Pages.Wallet.Models;

public sealed class WalletTagFormModel
{
    [Required, StringLength(40, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    // EPIC 27 Sprint 27.11: new/edited tags may only use one of the 10 official COR0-COR9
    // colors (03_DESIGN_DECISIONS.md §2) — no free HEX/RGB input. This is a Web-layer-only
    // restriction; Domain's WalletTag.NormalizeColor and the database check constraint remain
    // the permissive #RRGGBB superset so tags colored before this sprint keep rendering
    // correctly (docs/epics/25-design-system-brand-evolution/README.md §Sprint 25.11 already
    // declared user-persisted tag colors PRODUCT-SPECIFIC and out of scope for migration).
    [RegularExpression("^(#5247F9|#CE82FF|#58CC02|#1CB0F6|#FFB100|#FF7878|#FFFFFF|#ECECED|#100F3E|#DEFFF7)$", ErrorMessage = "Choose one of the 10 official colors.")]
    public string Color { get; set; } = "#5247F9";
}
