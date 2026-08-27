using BeeDayLab.Web.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BeeDayLab.Web.Components.DesignSystem.Feedback;

/// <summary>
/// Adds server-side logging to the stock <see cref="ErrorBoundary"/> via its documented
/// <see cref="OnErrorAsync"/> extension point — <c>CurrentException</c> is <c>protected</c>, so a
/// wrapping/composed component cannot read it; overriding is the only supported way in. Used only
/// by <see cref="BeeDayErrorBoundary"/>, which supplies the branded fallback content.
/// </summary>
public sealed class LoggingErrorBoundary : ErrorBoundary
{
    [Inject] private ILogger<LoggingErrorBoundary> Logger { get; set; } = default!;

    protected override Task OnErrorAsync(Exception exception)
    {
        Logger.LogError(WebEventIds.CircuitError, exception, "Unhandled exception caught by BeeDayErrorBoundary.");
        return base.OnErrorAsync(exception);
    }
}
