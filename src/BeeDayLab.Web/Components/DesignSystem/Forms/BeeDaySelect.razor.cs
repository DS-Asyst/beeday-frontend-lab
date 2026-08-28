using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BeeDayLab.Web.Components.DesignSystem.Forms;

public partial class BeeDaySelect<TValue>
{
    [Parameter, EditorRequired] public string Id { get; set; } = string.Empty;
    [Parameter] public string Label { get; set; } = string.Empty;
    [Parameter] public string FieldCssClass { get; set; } = "beeday-field";
    [Parameter] public string LabelCssClass { get; set; } = "beeday-field__label";
    [Parameter] public string InputCssClass { get; set; } = "beeday-field__control";
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ShowValidationMessage { get; set; } = true;
    [Parameter] public TValue Value { get; set; } = default!;
    [Parameter] public EventCallback<TValue> ValueChanged { get; set; }
    [Parameter, EditorRequired] public Expression<Func<TValue>> ValueExpression { get; set; } = default!;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private string ValidationMessageId => $"{Id}-validation";
}
