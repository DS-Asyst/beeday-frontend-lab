using BeeDayLab.Web.Emails;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Sprint 33.15 (FE33-101/102/103): parity/source tests for <see cref="TransactionalEmailTemplateCatalog"/>
/// — the Lab adaptation of <c>BeeDay.Infrastructure.Identity.IdentityEmailComposer</c>.
/// </summary>
public sealed class EmailTemplateCatalogTests
{
    [Theory]
    [InlineData(TransactionalEmailKind.Confirmation, "en-US", "Confirm your beeday email", "Confirm email")]
    [InlineData(TransactionalEmailKind.Confirmation, "pt-BR", "Confirme seu e-mail beeday", "Confirmar e-mail")]
    [InlineData(TransactionalEmailKind.PasswordReset, "en-US", "Reset your beeday password", "Reset password")]
    [InlineData(TransactionalEmailKind.PasswordReset, "pt-BR", "Redefina sua senha beeday", "Redefinir senha")]
    public void Compose_ResolvesLocalizedSubjectAndActionLabel(
        TransactionalEmailKind kind, string culture, string expectedSubject, string expectedActionLabel)
    {
        var preview = TransactionalEmailTemplateCatalog.Compose(kind, culture);

        Assert.Equal(expectedSubject, preview.Subject);
        Assert.Contains(expectedActionLabel, preview.Html, StringComparison.Ordinal);
        Assert.Contains(expectedActionLabel, preview.PlainText, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(TransactionalEmailKind.Confirmation)]
    [InlineData(TransactionalEmailKind.PasswordReset)]
    public void Compose_ActionUrlIsSyntheticAndNeverResolvesARealHost(TransactionalEmailKind kind)
    {
        var preview = TransactionalEmailTemplateCatalog.Compose(kind, "en-US");

        Assert.StartsWith("https://beeday-lab.invalid/", preview.ActionUrl, StringComparison.Ordinal);
        Assert.DoesNotContain("beeday.app", preview.ActionUrl, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("token=lab-preview-token-not-real", preview.ActionUrl, StringComparison.Ordinal);
        Assert.Contains(preview.ActionUrl, preview.Html, StringComparison.Ordinal);
        Assert.Contains(preview.ActionUrl, preview.PlainText, StringComparison.Ordinal);
    }

    [Fact]
    public void Compose_HtmlPreservesProductionShellAndApprovedBrandColor()
    {
        var preview = TransactionalEmailTemplateCatalog.Compose(TransactionalEmailKind.Confirmation, "en-US");

        Assert.Contains("<!doctype html>", preview.Html, StringComparison.Ordinal);
        Assert.Contains("#5247F9", preview.Html, StringComparison.Ordinal);
        Assert.Contains(">beeday<", preview.Html, StringComparison.Ordinal);
        Assert.Contains("role=\"presentation\"", preview.Html, StringComparison.Ordinal);
        Assert.Contains("Hello, Alex Rivera!", preview.Html, StringComparison.Ordinal);
    }

    [Fact]
    public void Compose_PlainTextIsAFullAlternativeToTheHtmlBody()
    {
        var preview = TransactionalEmailTemplateCatalog.Compose(TransactionalEmailKind.PasswordReset, "pt-BR");

        Assert.Contains("beeday", preview.PlainText, StringComparison.Ordinal);
        Assert.Contains("Olá, Alex Rivera!", preview.PlainText, StringComparison.Ordinal);
        Assert.Contains("Redefinir senha:", preview.PlainText, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("xx-XX")]
    [InlineData("")]
    public void Compose_UnsupportedOrEmptyCultureFallsBackToEnglish(string culture)
    {
        var preview = TransactionalEmailTemplateCatalog.Compose(TransactionalEmailKind.Confirmation, culture);

        Assert.Equal("Confirm your beeday email", preview.Subject);
    }

    [Fact]
    public void Compose_NeverUsesNonDeterministicPrimitives()
    {
        var first = TransactionalEmailTemplateCatalog.Compose(TransactionalEmailKind.Confirmation, "en-US");
        var second = TransactionalEmailTemplateCatalog.Compose(TransactionalEmailKind.Confirmation, "en-US");

        Assert.Equal(first, second);
    }
}
