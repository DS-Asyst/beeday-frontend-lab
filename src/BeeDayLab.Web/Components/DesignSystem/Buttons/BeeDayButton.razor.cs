using BeeDayLab.Web.Components.DesignSystem.Icons;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BeeDayLab.Web.Components.DesignSystem.Buttons;

public partial class BeeDayButton
{
    [Parameter] public BeeDayButtonVariant Variant { get; set; } = BeeDayButtonVariant.Primary;
    [Parameter] public string Type { get; set; } = "button";
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public bool FullWidth { get; set; }
    [Parameter] public bool Compact { get; set; }
    [Parameter] public BeeDayIconName? Icon { get; set; }
    [Parameter] public BeeDayIconSize IconSize { get; set; } = BeeDayIconSize.Small;
    [Parameter] public string? Class { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClasses
    {
        get
        {
            var variantClass = Variant switch
            {
                BeeDayButtonVariant.Primary => "beeday-button--primary",
                BeeDayButtonVariant.Secondary => "beeday-button--secondary",
                BeeDayButtonVariant.Success => "beeday-button--success",
                BeeDayButtonVariant.Warning => "beeday-button--warning",
                BeeDayButtonVariant.Back => "beeday-button--back",
                BeeDayButtonVariant.Danger => "beeday-button--danger",
                BeeDayButtonVariant.ConfirmationDanger => "beeday-button--confirmation-danger",
                BeeDayButtonVariant.ConfirmationCancel => "beeday-button--confirmation-cancel",
                BeeDayButtonVariant.ImportantWhite => "beeday-button--important-white",
                _ => "beeday-button--primary"
            };

            return string.Join(' ', new[]
            {
                "beeday-button",
                variantClass,
                IsLoading ? "beeday-button--loading" : null,
                FullWidth ? "beeday-button--full-width" : null,
                Compact ? "beeday-button--compact" : null,
                Class
            }.Where(value => !string.IsNullOrWhiteSpace(value)));
        }
    }

    private bool IsDisabled => Disabled || IsLoading;

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (IsDisabled || !OnClick.HasDelegate)
        {
            return;
        }

        await OnClick.InvokeAsync(args);
    }
}
