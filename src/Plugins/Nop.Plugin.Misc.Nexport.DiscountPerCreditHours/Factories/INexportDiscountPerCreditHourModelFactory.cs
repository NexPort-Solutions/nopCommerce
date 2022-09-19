using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Models.Plugins;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Factories
{
    public interface INexportDiscountPerCreditHourModelFactory
    {
        Task<DiscountPerCreditHoursPluginResourceListModel> PrepareDiscountPerCreditHourPluginResourceListModelAsync(
            DiscountPerCreditHoursPluginResourceListSearchModel searchModel);
    }
}
