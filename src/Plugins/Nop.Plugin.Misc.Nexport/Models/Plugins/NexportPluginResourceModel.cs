using System.Collections.Generic;
using Nop.Core.Domain.Localization;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Plugins;

public record NexportPluginResourceModel : BaseNopModel
{
    public IList<LocaleStringResource> ModifiedResources { get; set; } = new List<LocaleStringResource>();
}