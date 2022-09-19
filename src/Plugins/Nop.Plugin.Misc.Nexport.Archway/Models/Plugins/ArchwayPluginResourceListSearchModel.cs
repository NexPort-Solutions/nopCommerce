using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Archway.Models.Plugins;

public record ArchwayPluginResourceListSearchModel : BaseSearchModel
{
    [NopResourceDisplayName("Admin.Configuration.Plugins.Fields.FriendlyName")]
    public string FriendlyName { get; set; }
}
