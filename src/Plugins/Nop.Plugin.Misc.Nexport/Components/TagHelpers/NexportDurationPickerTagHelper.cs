using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Nop.Plugin.Misc.Nexport.Models.Components;

namespace Nop.Plugin.Misc.Nexport.Components.TagHelpers
{
    [SuppressMessage("ReSharper", "Mvc.PartialViewNotResolved")]
    [HtmlTargetElement("nex-duration-picker", Attributes = ForAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    public class NexportDurationPickerTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string IdAttributeName = "asp-id";
        private const string DisabledAttributeName = "asp-disabled";
        private const string RequiredAttributeName = "asp-required";
        private const string ValueAttributeName = "asp-value";

        private readonly IHtmlHelper _htmlHelper;

        /// <summary>
        /// HtmlGenerator
        /// </summary>
        protected IHtmlGenerator Generator { get; set; }

        /// <summary>
        /// An expression to be evaluated against the current model
        /// </summary>
        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName(IdAttributeName)]
        public string Id { get; set; }

        /// <summary>
        /// Indicates whether the field is disabled
        /// </summary>
        [HtmlAttributeName(DisabledAttributeName)]
        public string IsDisabled { set; get; }

        /// <summary>
        /// Indicates whether the field is required
        /// </summary>
        [HtmlAttributeName(RequiredAttributeName)]
        public string IsRequired { set; get; }

        /// <summary>
        /// The value of the element
        /// </summary>
        [HtmlAttributeName(ValueAttributeName)]
        public string Value { set; get; }

        /// <summary>
        /// ViewContext
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public NexportDurationPickerTagHelper(IHtmlGenerator generator, IHtmlHelper htmlHelper)
        {
            Generator = generator;
            _htmlHelper = htmlHelper;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (output == null)
                throw new ArgumentNullException(nameof(output));

            var viewContextAware = _htmlHelper as IViewContextAware;
            viewContextAware?.Contextualize(ViewContext);

            var elementId = string.IsNullOrWhiteSpace(Id) ? $"{For.Metadata.Name}-duration-picker" : Id;

            var viewModel = new NexportDurationPickerModel(elementId, For.Metadata.Name);
            if (!string.IsNullOrWhiteSpace(Value))
                viewModel.Value = Value;

            var content = await _htmlHelper.PartialAsync("~/Plugins/Misc.Nexport/Views/Shared/Components/TagHelpers/NexportDurationPicker/Default.cshtml", viewModel);
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", "nex-duration-picker");
            output.Attributes.SetAttribute("id", elementId);
            output.Content.SetHtmlContent(content);
        }
    }
}
