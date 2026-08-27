using Microsoft.Extensions.Logging;

namespace BeeDayLab.Web.Diagnostics;

public static class WebEventIds
{
    public static readonly EventId RequestFailed = new(6100, nameof(RequestFailed));
    public static readonly EventId CircuitError = new(6101, nameof(CircuitError));
}
