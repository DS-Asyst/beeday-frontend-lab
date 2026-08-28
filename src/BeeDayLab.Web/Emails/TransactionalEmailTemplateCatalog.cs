using System.Globalization;
using System.Net;
using System.Resources;

namespace BeeDayLab.Web.Emails;

/// <summary>Which of the 2 real transactional email flows to preview (Sprint 33.15, FE33-101/102).</summary>
public enum TransactionalEmailKind
{
    Confirmation,
    PasswordReset,
}

/// <summary>
/// One composed preview: the resolved subject, the full HTML document, the plain-text alternative,
/// the action URL shown/asserted separately so tests don't need to parse it back out of the HTML,
/// and the synthetic From/To envelope fields (Sprint 33.18-R "email client review mode" — Lab-only
/// display metadata, never part of <see cref="Html"/>/<see cref="PlainText"/> themselves).
/// </summary>
public sealed record TransactionalEmailPreview(string From, string To, string Subject, string Html, string PlainText, string ActionUrl);

/// <summary>
/// Lab adaptation of <c>BeeDay.Infrastructure.Identity.IdentityEmailComposer</c> — Sprint 33.15,
/// FE33-101/102/103. The HTML/plain-text template shell and every design token (colors, font
/// stacks, table-based layout) are copied verbatim (FE33-103, COPY): email clients cannot consume
/// CSS custom properties, so the same hex literals production tracks against
/// docs/design-system/01-foundations.md apply unchanged here.
///
/// What is NOT ported (FE33-101/102, MOCK): <c>IdentityEmailOptions</c> (real
/// <c>PublicBaseUrl</c>/<c>ConfirmationPath</c>/<c>PasswordResetPath</c> configuration),
/// <c>BeeDay.Domain.Enums.UserLanguage</c>, and any real recipient/token. Recipient, display name,
/// and token below are fixed, obviously-synthetic values; the action URL points at
/// <c>beeday-lab.invalid</c> (IANA/RFC 2606 reserved "never resolves" TLD) so no preview link can
/// ever be mistaken for, or accidentally reach, a real beeday endpoint.
/// </summary>
public static class TransactionalEmailTemplateCatalog
{
    private static readonly ResourceManager Resources = new(
        "BeeDayLab.Web.Emails.EmailResources",
        typeof(EmailResources).Assembly);

    private static readonly CultureInfo EnglishCulture = CultureInfo.GetCultureInfo("en-US");
    private static readonly CultureInfo PortugueseCulture = CultureInfo.GetCultureInfo("pt-BR");

    private const string SyntheticRecipient = "demo.reader@beeday-lab.invalid";
    private const string SyntheticDisplayName = "Alex Rivera";
    private const string SyntheticToken = "lab-preview-token-not-real";
    private const string PreviewBaseUrl = "https://beeday-lab.invalid/";

    // Sprint 33.18-R: envelope "From" for the Lab's review-mode chrome only — production's real
    // sender address/name come from Resend configuration (BEEDAY_RESEND_FROM_ADDRESS/_NAME), never
    // read or reproduced here. Obviously synthetic, same .invalid host as every other Lab email URL.
    private const string SyntheticSender = "beeday Lab <noreply@beeday-lab.invalid>";

    private sealed record EmailContentKeys(
        string PreheaderKey, string TitleKey, string IntroductionKey, string FooterKey, string ActionLabelKey, string Path);

    private static readonly EmailContentKeys ConfirmationKeys = new(
        "ConfirmationPreheader", "ConfirmationTitle", "ConfirmationIntroduction", "ConfirmationFooter",
        "ConfirmationActionLabel", "account/confirm-email");

    private static readonly EmailContentKeys ResetKeys = new(
        "ResetPreheader", "ResetTitle", "ResetIntroduction", "ResetFooter",
        "ResetActionLabel", "account/reset-password");

    private sealed record EmailContent(
        string Preheader, string Title, string Greeting, string Introduction,
        string ActionLabel, string ActionUrl, string FallbackLinkIntro, string Footer);

    public static TransactionalEmailPreview Compose(TransactionalEmailKind kind, string cultureCode)
    {
        var keys = kind == TransactionalEmailKind.Confirmation ? ConfirmationKeys : ResetKeys;
        var culture = ResolveCulture(cultureCode);
        var content = new EmailContent(
            Preheader: GetString(keys.PreheaderKey, culture),
            Title: GetString(keys.TitleKey, culture),
            Greeting: string.Format(culture, GetString("Greeting", culture), SyntheticDisplayName),
            Introduction: GetString(keys.IntroductionKey, culture),
            ActionLabel: GetString(keys.ActionLabelKey, culture),
            ActionUrl: BuildPreviewUrl(keys.Path),
            FallbackLinkIntro: GetString("FallbackLinkIntro", culture),
            Footer: GetString(keys.FooterKey, culture));

        return new TransactionalEmailPreview(
            SyntheticSender,
            SyntheticRecipient,
            content.Title,
            BuildHtmlTemplate(culture, content),
            BuildPlainTextTemplate(content),
            content.ActionUrl);
    }

    private static CultureInfo ResolveCulture(string cultureCode) =>
        string.Equals(cultureCode, "pt-BR", StringComparison.OrdinalIgnoreCase) ? PortugueseCulture : EnglishCulture;

    private static string GetString(string name, CultureInfo culture) =>
        Resources.GetString(name, culture)
            ?? throw new InvalidOperationException($"Missing transactional email resource '{name}'.");

    private static string BuildPreviewUrl(string path) =>
        $"{PreviewBaseUrl}{path}?token={Uri.EscapeDataString(SyntheticToken)}";

    // Colors below mirror docs/design-system/01-foundations.md §2 (verified 2026-08-16) — the same
    // Experience System token values production's composer tracks. #5247F9 is the single approved
    // beeday Brand Color (CLAUDE.md §5.1). Text-secondary uses #514858 rather than the deferred
    // --beeday-color-text-muted (#817789, ~4.26:1 on white, below WCAG AA) for the same accessibility
    // reason production's composer documents (EPIC 28, Sprint 28.9 / DEFER 25.15).
    private const string BrandColor = "#5247F9";
    private const string CanvasColor = "#F7F7F7";
    private const string SurfaceColor = "#FFFFFF";
    private const string TextPrimaryColor = "#2F2737";
    private const string TextSecondaryColor = "#514858";
    private const string BorderColor = "#E5E5E5";

    // Table-based, fully inline-styled layout — copied verbatim from production. Email clients are
    // not browsers: no CSS custom properties, no guaranteed remote font, and Outlook's desktop
    // renderer needs table layout + role="presentation" to lay out predictably. No remote image is
    // used anywhere, so the template is fully legible with images off and with custom fonts blocked.
    private static string BuildHtmlTemplate(CultureInfo culture, EmailContent content)
    {
        var safePreheader = WebUtility.HtmlEncode(content.Preheader);
        var safeTitle = WebUtility.HtmlEncode(content.Title);
        var safeGreeting = WebUtility.HtmlEncode(content.Greeting);
        var safeIntroduction = WebUtility.HtmlEncode(content.Introduction);
        var safeActionLabel = WebUtility.HtmlEncode(content.ActionLabel);
        var safeActionUrl = WebUtility.HtmlEncode(content.ActionUrl);
        var safeFallbackLinkIntro = WebUtility.HtmlEncode(content.FallbackLinkIntro);
        var safeFooter = WebUtility.HtmlEncode(content.Footer);
        var productFontStack = "'Nunito','Segoe UI',Arial,sans-serif";
        var brandFontStack = "'Coiny','Nunito','Segoe UI',sans-serif";

        return $$"""
        <!doctype html>
        <html lang="{{culture.Name}}">
        <head>
        <meta charset="utf-8">
        <meta name="viewport" content="width=device-width, initial-scale=1">
        <meta http-equiv="X-UA-Compatible" content="IE=edge">
        <title>{{safeTitle}}</title>
        </head>
        <body style="margin:0;padding:0;background:{{CanvasColor}};">
        <div style="display:none;font-size:1px;line-height:1px;max-height:0;max-width:0;opacity:0;overflow:hidden;mso-hide:all;">{{safePreheader}}</div>
        <div style="display:none;font-size:1px;line-height:1px;max-height:0;max-width:0;opacity:0;overflow:hidden;mso-hide:all;">&#8203;&#847; &#8203;&#847; &#8203;&#847; &#8203;&#847;</div>
        <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:{{CanvasColor}};">
        <tr>
        <td align="center" style="padding:32px 16px;">
        <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:560px;background:{{SurfaceColor}};border-radius:12px;">
        <tr>
        <td align="center" style="padding:32px 32px 8px;">
        <span style="font-family:{{brandFontStack}};font-size:28px;line-height:1;color:{{BrandColor}};">beeday</span>
        </td>
        </tr>
        <tr>
        <td style="padding:16px 32px 0;font-family:{{productFontStack}};">
        <h1 style="margin:0 0 20px;font-size:20px;line-height:1.3;font-weight:700;color:{{TextPrimaryColor}};">{{safeTitle}}</h1>
        <p style="margin:0 0 16px;font-size:15px;line-height:1.5;color:{{TextPrimaryColor}};">{{safeGreeting}}</p>
        <p style="margin:0 0 28px;font-size:15px;line-height:1.5;color:{{TextSecondaryColor}};">{{safeIntroduction}}</p>
        <table role="presentation" cellpadding="0" cellspacing="0" style="margin:0 0 28px;">
        <tr>
        <td style="border-radius:8px;background:{{BrandColor}};">
        <a href="{{safeActionUrl}}" style="display:inline-block;padding:14px 24px;font-family:{{productFontStack}};font-size:15px;font-weight:700;color:#FFFFFF;text-decoration:none;">{{safeActionLabel}}</a>
        </td>
        </tr>
        </table>
        <p style="margin:0 0 6px;font-size:13px;line-height:1.5;color:{{TextSecondaryColor}};">{{safeFallbackLinkIntro}}</p>
        <p style="margin:0 0 28px;font-size:13px;line-height:1.5;word-break:break-all;"><a href="{{safeActionUrl}}" style="color:{{BrandColor}};">{{safeActionUrl}}</a></p>
        <p style="margin:0 0 24px;font-size:13px;line-height:1.5;color:{{TextSecondaryColor}};">{{safeFooter}}</p>
        </td>
        </tr>
        <tr>
        <td style="padding:16px 32px 32px;border-top:1px solid {{BorderColor}};font-family:{{productFontStack}};">
        <p style="margin:0;font-size:12px;line-height:1.5;color:{{TextSecondaryColor}};">beeday</p>
        </td>
        </tr>
        </table>
        </td>
        </tr>
        </table>
        </body>
        </html>
        """;
    }

    private static string BuildPlainTextTemplate(EmailContent content) =>
        $"""
        beeday

        {content.Title}

        {content.Greeting}

        {content.Introduction}

        {content.ActionLabel}: {content.ActionUrl}

        {content.FallbackLinkIntro}
        {content.ActionUrl}

        {content.Footer}
        """;
}
