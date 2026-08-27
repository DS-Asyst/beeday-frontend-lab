using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BeeDayLab.Web.Components.DesignSystem.Layout;

public partial class BeeDaySettingsForm<TModel>
{
    [Parameter, EditorRequired] public TModel Model { get; set; } = default!;
    [Parameter, EditorRequired] public string FormName { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string SubmitLabel { get; set; } = string.Empty;
    [Parameter] public bool IsBusy { get; set; }
    [Parameter] public string? SubmitButtonClass { get; set; }
    [Parameter] public EventCallback OnValidSubmit { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private Task HandleValidSubmitAsync(EditContext _) => OnValidSubmit.InvokeAsync();
}
