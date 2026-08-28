namespace BeeDayLab.Web.Components.Pages.Daily.Experience.Feedback;

/// <summary>
/// COPY (Sprint 33.13, FE33-096) of BeeDay.Web's
/// <c>Components/Features/Experience/Feedback/BeeDayFeedbackStore.cs</c> — verbatim apart from the
/// namespace. Verified on reading: zero BeeDay dependency, pure in-memory event history (a dedupe
/// set, a current item and a three-entry rolling history), so it is copied rather than adapted.
///
/// <para>What is NOT ported is its production feeder, <c>BeeDayFeedbackEventHandler</c>: that is a
/// real <c>MediatR.INotificationHandler&lt;DomainEventNotification&gt;</c> reacting to a
/// <c>UserLeveledUpDomainEvent</c> — EXCLUDE under ADR-008, and explicitly forbidden by this
/// Sprint's boundary. <c>LabDashboardState</c> calls <see cref="Add"/> directly with synthetic
/// level-up data instead.</para>
///
/// <para>Registered <c>Scoped</c> in Program.cs: the store holds per-circuit UI state (the modal
/// currently showing and the recent history behind it), the same lifetime reasoning as
/// <c>ToastService</c>/<c>ScenarioSelection</c>, and matching production's own registration.</para>
/// </summary>
public sealed class BeeDayFeedbackStore
{
    private readonly HashSet<Guid> processedEntries = [];
    private readonly List<BeeDayFeedback> history = [];

    public event Action? Changed;

    public BeeDayFeedback? Current { get; private set; }

    public IReadOnlyList<BeeDayFeedback> History => history;

    public void Add(BeeDayFeedback feedback)
    {
        ArgumentNullException.ThrowIfNull(feedback);

        if (!processedEntries.Add(feedback.ExperienceEntryId))
        {
            return;
        }

        Current = feedback;
        history.Insert(0, feedback);
        if (history.Count > 3)
        {
            history.RemoveAt(history.Count - 1);
        }

        Changed?.Invoke();
    }

    public void Consume()
    {
        if (Current is null)
        {
            return;
        }

        Current = null;
        Changed?.Invoke();
    }
}
