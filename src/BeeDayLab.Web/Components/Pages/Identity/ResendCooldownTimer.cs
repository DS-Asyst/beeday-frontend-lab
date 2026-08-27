namespace BeeDayLab.Web.Components.Pages.Identity;

/// <summary>
/// Lab COPY, verbatim (Sprint 33.12, FE33-081 dependency), of BeeDay.Web's
/// Components/Features/Identity/ResendCooldownTimer.cs — 100% presentation-only (a plain
/// PeriodicTimer countdown helper), zero BeeDay dependency, so it needed no adaptation at all beyond
/// the namespace move. Shared by ResendConfirmation.razor and EmailConfirmationSent.razor, same
/// grouping production uses.
/// </summary>
public sealed class ResendCooldownTimer(Func<Task> onTick) : IDisposable
{
    private PeriodicTimer? timer;
    private CancellationTokenSource? cts;

    public int SecondsRemaining { get; private set; }

    public void Start(int seconds = 60)
    {
        SecondsRemaining = seconds;
        cts?.Cancel();
        cts?.Dispose();
        timer?.Dispose();
        cts = new CancellationTokenSource();
        timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        _ = RunAsync(cts.Token);
    }

    private async Task RunAsync(CancellationToken token)
    {
        try
        {
            while (SecondsRemaining > 0 && await timer!.WaitForNextTickAsync(token))
            {
                SecondsRemaining--;
                await onTick();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Dispose()
    {
        cts?.Cancel();
        cts?.Dispose();
        timer?.Dispose();
    }
}
