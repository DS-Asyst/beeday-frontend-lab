using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace BeeDayLab.Web.Components.DesignSystem.Forms;

public partial class BeeDayCheckbox
{
    [Parameter, EditorRequired] public string Id { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string Label { get; set; } = string.Empty;
    [Parameter] public string FieldCssClass { get; set; } = "beeday-checkbox";
    [Parameter] public string LabelCssClass { get; set; } = "beeday-checkbox__label";
    [Parameter] public string InputCssClass { get; set; } = "beeday-checkbox__control";
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ShowValidationMessage { get; set; } = true;
    [Parameter] public bool Value { get; set; }
    [Parameter] public EventCallback<bool> ValueChanged { get; set; }
    [Parameter, EditorRequired] public Expression<Func<bool>> ValueExpression { get; set; } = default!;
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private string ValidationMessageId => $"{Id}-validation";
}
