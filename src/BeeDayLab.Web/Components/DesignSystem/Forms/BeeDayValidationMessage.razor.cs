using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BeeDayLab.Web.Components.DesignSystem.Forms;

/// <summary>
/// Lab adaptation (Sprint 33.8, FE33-104): the BeeDay source additionally calls
/// <c>ValidationMessageLocalizer.Translate(message, Localizer)</c> on each message, mapping a
/// DataAnnotations validation string to its localized equivalent via
/// <c>IStringLocalizer&lt;DesignSystemResources&gt;</c>. Both the localizer injection and
/// <c>ValidationMessageLocalizer</c> itself are deliberately NOT ported: the localizer maps real
/// BeeDay business validation copy (password rules, name/title length, etc.) that does not exist in
/// the Lab, and porting that mapping would be exactly the "mock business logic" ADR-008 forbids.
/// The EditContext/FieldIdentifier subscription mechanism below is a standard ASP.NET Core Blazor
/// forms API (not a BeeDay dependency) and is ported as-is; messages are rendered directly from
/// <see cref="EditContext.GetValidationMessages(FieldIdentifier)"/> as plain strings.
/// </summary>
public partial class BeeDayValidationMessage<TValue> : IDisposable
{
    [CascadingParameter] private EditContext? EditContext { get; set; }
    [Parameter, EditorRequired] public Expression<Func<TValue>> For { get; set; } = default!;
    [Parameter] public string? Id { get; set; }

    private FieldIdentifier _fieldIdentifier;
    private IReadOnlyList<string> _messages = Array.Empty<string>();
    private EditContext? _subscribedContext;

    protected override void OnParametersSet()
    {
        if (EditContext is null)
        {
            throw new InvalidOperationException($"{nameof(BeeDayValidationMessage<TValue>)} requires a cascading EditContext.");
        }
        _fieldIdentifier = FieldIdentifier.Create(For);
        if (!ReferenceEquals(_subscribedContext, EditContext))
        {
            if (_subscribedContext is not null)
            {
                _subscribedContext.OnValidationStateChanged -= HandleValidationStateChanged;
            }
            _subscribedContext = EditContext;
            _subscribedContext.OnValidationStateChanged += HandleValidationStateChanged;
        }
        RefreshMessages();
    }

    private void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs args)
    {
        RefreshMessages();
        _ = InvokeAsync(StateHasChanged);
    }

    private void RefreshMessages() => _messages = EditContext?.GetValidationMessages(_fieldIdentifier)
        .ToArray() ?? Array.Empty<string>();

    public void Dispose()
    {
        if (_subscribedContext is not null)
        {
            _subscribedContext.OnValidationStateChanged -= HandleValidationStateChanged;
        }
    }
}
