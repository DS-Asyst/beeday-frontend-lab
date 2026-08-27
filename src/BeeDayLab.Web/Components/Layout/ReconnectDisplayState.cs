namespace BeeDayLab.Web.Components.Layout;

/// <summary>
/// Lab-local visual states for <see cref="ReconnectModal"/> (Sprint 33.9, FE33-050). The real
/// BeeDay component represents Blazor Server's SignalR circuit-reconnection UI, whose state
/// transitions are driven by <c>blazor.web.js</c>'s real circuit-loss/retry/pause machinery. The
/// Lab has no real circuit worth simulating with real reconnection JS interop, so this enum lets a
/// Lab caller/demo set a visual state directly — a MOCK of the five CSS-driven states the real
/// component's stylesheet already encodes (<c>components-reconnect-show</c>,
/// <c>components-reconnect-retrying</c>, <c>components-reconnect-failed</c>,
/// <c>components-reconnect-paused</c>, <c>components-reconnect-resume-failed</c>), plus a default
/// hidden state.
/// </summary>
public enum ReconnectDisplayState
{
    Hidden,
    Rejoining,
    Retrying,
    Failed,
    Paused,
    ResumeFailed
}
