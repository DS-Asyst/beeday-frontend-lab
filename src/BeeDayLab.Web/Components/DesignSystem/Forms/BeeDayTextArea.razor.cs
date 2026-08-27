using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BeeDayLab.Web.Components.DesignSystem.Forms;

public partial class BeeDayTextArea
{
    [Parameter, EditorRequired] public string Id { get; set; } = string.Empty;
    [Parameter] public string Label { get; set; } = string.Empty;
    [Parameter] public string Placeholder { get; set; } = string.Empty;
    [Parameter] public string FieldCssClass { get; set; } = "beeday-field";
    [Parameter] public string LabelCssClass { get; set; } = "beeday-field__label";
    [Parameter] public string InputCssClass { get; set; } = "beeday-field__control beeday-field__control--textarea";
    [Parameter] public string CounterCssClass { get; set; } = "beeday-field__counter";
    [Parameter] public int? MaxLength { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public bool ShowValidationMessage { get; set; } = true;
    [Parameter] public bool ShowCounter { get; set; }
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
    [Parameter, EditorRequired] public Expression<Func<string?>> ValueExpression { get; set; } = default!;
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private string ValidationMessageId => $"{Id}-validation";
}
