using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models.RegistrationField
{
    public class NexportRegistrationFieldCategoryModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugins.Misc.Nexport.RegistrationField.Category.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.RegistrationField.Category.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.RegistrationField.Category.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}
