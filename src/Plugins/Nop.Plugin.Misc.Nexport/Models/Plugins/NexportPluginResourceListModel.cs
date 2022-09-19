using System.Collections.Generic;
using Nop.Core.Domain.Localization;
using Nop.Web.Areas.Admin.Models.Localization;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Plugins
{

    public record NexportPluginResourceListModel : BasePagedListModel<LocaleResourceModel>
    {

    }   
}