namespace BeeDayLab.Web.Components.Pages.Emails;

/// <summary>
/// Marker type for the <c>/emails</c> preview page chrome (template/locale/width selectors, subject
/// and plain-text labels) via <c>IStringLocalizer&lt;EmailPreviewResources&gt;</c>. Distinct from
/// <c>BeeDayLab.Web.Emails.EmailResources</c>, which holds the actual email content mirrored from
/// production — this catalog is Lab-only page chrome with no production equivalent.
/// </summary>
public sealed class EmailPreviewResources;
