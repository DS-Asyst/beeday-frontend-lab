namespace BeeDayLab.Web.Components.Pages.Daily;

/// <summary>
/// Code-behind for <c>DailyHome.razor</c> (Sprint 33.13, FE33-090) — the Lab adaptation of
/// BeeDay.Web's <c>Components/Features/Dashboard/Pages/Home.razor.cs</c>.
///
/// <para>Production's version does three things here: subscribes to <c>State.Changed</c>, calls
/// <c>AuthenticatedUserInitializer.EnsureInitializedAsync()</c>, and — when the authenticated user
/// has no profile — probes <c>State.GetDataAsync()</c> to decide between redirecting to
/// <c>/profile/create</c> and, on <c>InvalidDomainStateException</c>, to <c>/login</c>. Only the
/// first survives: the initializer is real auth infrastructure and the redirect branch is driven by
/// a Domain exception type, both EXCLUDE under ADR-008, and neither has anything to key off in a Lab
/// with no account concept.</para>
/// </summary>
public partial class DailyHome : IDisposable
{
    protected override async Task OnInitializedAsync()
    {
        State.Changed += HandleStateChanged;
        await State.InitializeAsync();
    }

    private void HandleStateChanged() => _ = InvokeAsync(StateHasChanged);

    public void Dispose() => State.Changed -= HandleStateChanged;
}
