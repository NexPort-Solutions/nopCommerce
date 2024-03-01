using System.Threading.Tasks;
using Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Models;

namespace Nop.Plugin.Misc.Nexport.DiscountPerCreditHours.Factories;

public interface INexportDiscountPerCreditHourModelFactory
{
    Task<DiscountPerCreditHoursPluginResourceListModel> PrepareDiscountPerCreditHourPluginResourceListModelAsync(
        DiscountPerCreditHoursPluginResourceListSearchModel searchModel);
}