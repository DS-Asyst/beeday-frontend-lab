namespace BeeDayLab.Web.Components.DesignSystem.Feedback;

/// <summary>
/// Lab adaptation (Sprint 33.8, FE33-105): ported from BeeDay.Web's Services/ToastService.cs with
/// the <c>IStringLocalizer&lt;SharedResources&gt;</c> dependency dropped entirely — the Lab has no
/// localization pipeline (Program.cs has no AddLocalization), and the already-copied BeeDayIcon has
/// no localizer dependency either, so introducing one now for this single service would be scope
/// creep. Default titles are plain hardcoded English string literals instead of localizer keys,
/// taken verbatim from SharedResources.en-US.resx at extraction time. Same shape otherwise:
/// ShowSuccess/ShowError/ShowInfo/Remove, Messages, and the Changed event BeeDayToastHost subscribes
/// to. Registered scoped in Program.cs — stateful per-circuit, matching Blazor Server's per-circuit
/// scope lifetime.
/// </summary>
public sealed class ToastService
{
    private readonly List<ToastMessage> messages = [];

    public event Action? Changed;

    public IReadOnlyList<ToastMessage> Messages => messages;

    public void ShowSuccess(string message, string? title = null) =>
        Show(message, title ?? "Success", ToastVariant.Success);

    public void ShowError(string message, string? title = null) =>
        Show(message, title ?? "Something went wrong", ToastVariant.Error, TimeSpan.FromSeconds(7));

    public void ShowInfo(string message, string? title = null) =>
        Show(message, title ?? "Information", ToastVariant.Info);

    public void Remove(Guid id)
    {
        if (messages.RemoveAll(item => item.Id == id) > 0)
        {
            Changed?.Invoke();
        }
    }

    private void Show(
        string message,
        string title,
        ToastVariant variant,
        TimeSpan? duration = null)
    {
        var toast = new ToastMessage(Guid.NewGuid(), title, message, variant);
        messages.Add(toast);
        Changed?.Invoke();
        _ = RemoveAfterDelayAsync(toast.Id, duration ?? TimeSpan.FromSeconds(4));
    }

    private async Task RemoveAfterDelayAsync(Guid id, TimeSpan duration)
    {
        await Task.Delay(duration);
        Remove(id);
    }
}

public sealed record ToastMessage(
    Guid Id,
    string Title,
    string Message,
    ToastVariant Variant);

public enum ToastVariant
{
    Success,
    Error,
    Info
}
