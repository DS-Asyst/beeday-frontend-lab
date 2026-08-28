using System.ComponentModel.DataAnnotations;
using BeeDayLab.Web.Components.DesignSystem.Forms;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Xunit;

namespace BeeDayLab.Web.Tests;

/// <summary>
/// Deterministic bUnit component tests for Sprint 33.8 (FE33-019..022, FE33-104): proves the
/// Design System form inputs render their accessibility wiring (aria-describedby pointing at a
/// validation message id) and, specifically for the ADAPTed BeeDayValidationMessage, that it
/// renders EditContext.GetValidationMessages(...) directly with zero BeeDay-specific dependency —
/// no IStringLocalizer&lt;DesignSystemResources&gt; injection and no ValidationMessageLocalizer
/// business-message mapping (that mapping targets real BeeDay password/name/title validation copy
/// that does not exist in the Lab, so ADR-008 forbids porting it).
/// </summary>
public sealed class FormsAccessibilityTests
{
    [Fact]
    public void InputWiresAriaDescribedByToItsValidationMessageId()
    {
        using var context = new BunitContext();
        var model = new SampleModel();
        var editContext = new EditContext(model);

        var cut = context.Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(p => p.Value, editContext)
            .AddChildContent<BeeDayInput>(inputParameters => inputParameters
                .Add(p => p.Id, "name-field")
                .Add(p => p.Value, model.Name)
                .Add(p => p.ValueChanged, (string? value) => model.Name = value ?? string.Empty)
                .Add(p => p.ValueExpression, () => model.Name)));

        var input = cut.Find("#name-field");
        Assert.Equal("name-field-validation", input.GetAttribute("aria-describedby"));
    }

    [Fact]
    public void ValidationMessageRendersRoleAlertWithPlainEditContextMessageAndNoLocalizationMarkup()
    {
        using var context = new BunitContext();
        var model = new SampleModel { Name = string.Empty };
        var editContext = new EditContext(model);
        // BeeDayValidationMessage renders whatever EditContext.GetValidationMessages(...) returns,
        // with no DataAnnotationsValidator rendered in this focused test — a ValidationMessageStore
        // is the standard Blazor forms API for injecting a message deterministically, proving the
        // component's own rendering/subscription logic without depending on DataAnnotations
        // execution order.
        var messageStore = new ValidationMessageStore(editContext);
        messageStore.Add(editContext.Field(nameof(SampleModel.Name)), "Name is required.");
        editContext.NotifyValidationStateChanged();

        var cut = context.Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(p => p.Value, editContext)
            .AddChildContent<BeeDayValidationMessage<string>>(messageParameters => messageParameters
                .Add(p => p.For, () => model.Name)
                .Add(p => p.Id, "name-field-validation")));

        var alert = cut.Find("[role='alert']");
        Assert.Equal("Name is required.", alert.QuerySelector("span")!.TextContent);
    }

    [Fact]
    public void ValidationMessageRendersNothingWhenFieldIsValid()
    {
        using var context = new BunitContext();
        var model = new SampleModel { Name = "Ada" };
        var editContext = new EditContext(model);
        editContext.Validate();

        var cut = context.Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(p => p.Value, editContext)
            .AddChildContent<BeeDayValidationMessage<string>>(messageParameters => messageParameters
                .Add(p => p.For, () => model.Name)
                .Add(p => p.Id, "name-field-validation")));

        Assert.Empty(cut.FindAll("[role='alert']"));
    }

    [Fact]
    public void ValidationMessageThrowsWithoutACascadingEditContext()
    {
        using var context = new BunitContext();
        var model = new SampleModel();

        var act = () => context.Render<BeeDayValidationMessage<string>>(parameters => parameters
            .Add(p => p.For, () => model.Name)
            .Add(p => p.Id, "name-field-validation"));

        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void CheckboxRendersBothVisualStatesAndWiresAriaDescribedBy()
    {
        using var context = new BunitContext();
        var model = new SampleModel();
        var editContext = new EditContext(model);

        var cut = context.Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(p => p.Value, editContext)
            .AddChildContent<BeeDayCheckbox>(checkboxParameters => checkboxParameters
                .Add(p => p.Id, "accept-terms")
                .Add(p => p.Label, "Accept terms")
                .Add(p => p.Value, model.Accepted)
                .Add(p => p.ValueChanged, (bool value) => model.Accepted = value)
                .Add(p => p.ValueExpression, () => model.Accepted)));

        var input = cut.Find("#accept-terms");
        Assert.Equal("accept-terms-validation", input.GetAttribute("aria-describedby"));
        Assert.NotEmpty(cut.FindAll(".beeday-checkbox__unchecked"));
        Assert.NotEmpty(cut.FindAll(".beeday-checkbox__checked"));
    }

    [Fact]
    public void TextAreaRendersCounterWhenEnabledWithMaxLength()
    {
        using var context = new BunitContext();
        var model = new SampleModel { Name = "abc" };
        var editContext = new EditContext(model);

        var cut = context.Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(p => p.Value, editContext)
            .AddChildContent<BeeDayTextArea>(textAreaParameters => textAreaParameters
                .Add(p => p.Id, "notes")
                .Add(p => p.Value, model.Name)
                .Add(p => p.ValueChanged, (string? value) => model.Name = value ?? string.Empty)
                .Add(p => p.ValueExpression, () => model.Name)
                .Add(p => p.MaxLength, 100)
                .Add(p => p.ShowCounter, true)));

        Assert.Equal("3 / 100", cut.Find("small").TextContent);
    }

    [Fact]
    public void SelectRendersChildOptionsAndControlIcon()
    {
        using var context = new BunitContext();
        var model = new SampleModel { Name = "b" };
        var editContext = new EditContext(model);

        var cut = context.Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(p => p.Value, editContext)
            .AddChildContent<BeeDaySelect<string>>(selectParameters => selectParameters
                .Add(p => p.Id, "letter")
                .Add(p => p.Value, model.Name)
                .Add(p => p.ValueChanged, (string value) => model.Name = value)
                .Add(p => p.ValueExpression, () => model.Name)
                .AddChildContent("<option value=\"a\">A</option><option value=\"b\">B</option>")));

        Assert.Equal(2, cut.FindAll("option").Count);
        Assert.NotEmpty(cut.FindAll(".beeday-field__control-icon"));
    }

    private sealed class SampleModel
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = string.Empty;

        public bool Accepted { get; set; }
    }
}
