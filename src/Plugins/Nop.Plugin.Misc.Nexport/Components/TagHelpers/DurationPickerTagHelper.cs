using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Nop.Plugin.Misc.Nexport.Models.Components;

namespace Nop.Plugin.Misc.Nexport.Components.TagHelpers;

[HtmlTargetElement("nex-duration-picker", Attributes = FOR_ATTRIBUTE_NAME, TagStructure = TagStructure.WithoutEndTag)]
public class DurationPickerTagHelper : TagHelper
{
    private const string FOR_ATTRIBUTE_NAME = "asp-for";
    private const string ID_ATTRIBUTE_NAME = "asp-id";
    private const string DISABLED_ATTRIBUTE_NAME = "asp-disabled";
    private const string REQUIRED_ATTRIBUTE_NAME = "asp-required";
    private const string VALUE_ATTRIBUTE_NAME = "asp-value";

    private readonly IHtmlHelper _htmlHelper;

    /// <summary>
    /// HtmlGenerator
    /// </summary>
    protected IHtmlGenerator Generator { get; set; }

    /// <summary>
    /// An expression to be evaluated against the current model
    /// </summary>
    [HtmlAttributeName(FOR_ATTRIBUTE_NAME)]
    public required ModelExpression For { get; set; }

    [HtmlAttributeName(ID_ATTRIBUTE_NAME)]
    public string? Id { get; set; }

    /// <summary>
    /// Indicates whether the field is disabled
    /// </summary>
    [HtmlAttributeName(DISABLED_ATTRIBUTE_NAME)]
    public string? IsDisabled { set; get; }

    /// <summary>
    /// Indicates whether the field is required
    /// </summary>
    [HtmlAttributeName(REQUIRED_ATTRIBUTE_NAME)]
    public string? IsRequired { set; get; }

    /// <summary>
    /// The value of the element
    /// </summary>
    [HtmlAttributeName(VALUE_ATTRIBUTE_NAME)]
    public string? Value { set; get; }

    /// <summary>
    /// ViewContext
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext? ViewContext { get; set; }

    public DurationPickerTagHelper(IHtmlGenerator generator, IHtmlHelper htmlHelper)
    {
        Generator = generator;
        _htmlHelper = htmlHelper;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (For.Metadata.Name is null)
        {
            throw new InvalidOperationException($"{nameof(FOR_ATTRIBUTE_NAME)} must be a property.");
        }
        var viewContextAware = _htmlHelper as IViewContextAware;
        viewContextAware?.Contextualize(ViewContext);
        var elementId = string.IsNullOrWhiteSpace(Id) ? $"{For.Metadata.Name}-duration-picker" : Id;
        var viewModel = new DurationPickerModel(elementId, For.Metadata.Name);
        if (!string.IsNullOrWhiteSpace(Value))
        {
            viewModel.Value = Value;
        }
        var content = await _htmlHelper.PartialAsync("~/Plugins/Misc.Nexport/Views/Shared/Components/TagHelpers/DurationPicker/Default.cshtml", viewModel);
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", "nex-duration-picker");
        output.Attributes.SetAttribute("id", elementId);
        _ = output.Content.SetHtmlContent(content);
    }
}
