namespace BeeDayLab.Web.Components.Pages.Preview;

/// <summary>
/// The canonical page index consumed by the Page + Email Gallery (Sprint 33.17, Issue #378,
/// Required Work item 1: "Create page index by product area"). One entry per <c>@page</c> route
/// already extracted by Sprints 33.6-33.15 — <c>PreviewPageRegistryTests</c> (Lab) proves this
/// registry stays exhaustive by cross-checking it against every <c>@page</c> directive actually
/// declared under <c>Components/Pages/</c>.
/// </summary>
public static class PreviewPageRegistry
{
    public static readonly PreviewPageEntry[] Public =
    [
        new("/", "Home"),
        new("/mission", "Mission"),
        new("/efficacy", "Efficacy"),
        new("/brand-guidelines", "Brand Guidelines"),
        new("/contact", "Contact"),
        new("/beeday", "Product"),
        new("/beeday-plus", "Product Plus"),
        new("/android", "Android"),
        new("/ios", "iOS"),
        new("/faqs", "FAQs"),
        new("/community-guidelines", "Community Guidelines"),
        new("/terms", "Terms"),
        new("/privacy", "Privacy"),
        new("/brand/typography", "Typography Guidelines"),
        new("/experience-system/brand/typography", "Typography Guidelines (Experience System route)"),
        new("/experience-system", "Experience System Home"),
        new("/experience-system/brand", "Brand Overview"),
        new("/experience-system/brand/identity", "Brand Identity"),
        new("/experience-system/brand/wordmark", "Brand Wordmark"),
        new("/experience-system/brand/color", "Brand Color"),
        new("/experience-system/brand/illustration", "Brand Illustration"),
        new("/experience-system/brand/characters", "Brand Characters"),
        new("/experience-system/brand/writing", "Brand Writing"),
        new("/experience-system/ui", "UI Overview"),
        new("/experience-system/ui/foundations", "UI Foundations"),
        new("/experience-system/ui/components", "UI Components"),
        new("/experience-system/ui/product-patterns", "UI Product Patterns"),
        new("/experience-system/ui/interaction", "UI Interaction"),
        new("/experience-system/ui/layout", "UI Layout"),
        new("/experience-system/ux", "UX Overview"),
        new("/experience-system/ux/accessibility", "UX Accessibility"),
        new("/experience-system/ux/responsive", "UX Responsive"),
        new("/experience-system/ux/localization", "UX Localization"),
        new("/experience-system/ux/motion", "UX Motion"),
        new("/experience-system/ux/performance", "UX Performance"),
    ];

    public static readonly PreviewPageEntry[] Identity =
    [
        new("/login", "Login"),
        new("/welcome", "Welcome"),
        new("/profile/create", "Create Profile"),
        new("/profile/create?authenticated=true", "Create Profile (authenticated)"),
        new("/account/forgot-password", "Forgot Password"),
        new("/account/resend-confirmation", "Resend Confirmation"),
        new("/account/email-confirmation-sent", "Email Confirmation Sent"),
        new("/account/confirm-email", "Confirm Email"),
        new("/account/reset-password", "Reset Password"),
        new("/onboarding/tutorial", "Onboarding Tutorial"),
        new("/identity/redirect-to-login-preview", "Redirect To Login (preview)"),
    ];

    public static readonly PreviewPageEntry[] Account =
    [
        new("/account", "Account"),
        new("/settings", "Settings"),
    ];

    public static readonly PreviewPageEntry[] Daily =
    [
        new("/profile", "Profile Home"),
        new("/daily", "Daily Dashboard"),
    ];

    public static readonly PreviewPageEntry[] Wallet =
    [
        new("/wallet", "Wallet"),
    ];

    public static readonly PreviewPageEntry[] Email =
    [
        new("/emails?template=confirmation", "Email Confirmation"),
        new("/emails?template=reset", "Password Reset Email"),
    ];

    public static readonly PreviewPageEntry[] System =
    [
        new("/not-found", "Not Found"),
        new("/Error", "Error"),
    ];

    public static readonly PreviewPageEntry[] All =
    [
        .. Public,
        .. Identity,
        .. Account,
        .. Daily,
        .. Wallet,
        .. Email,
        .. System,
    ];
}
