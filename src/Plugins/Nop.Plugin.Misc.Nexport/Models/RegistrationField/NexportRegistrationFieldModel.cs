using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Plugin.Misc.Nexport.Domain.Enums;

namespace Nop.Plugin.Misc.Nexport.Models.RegistrationField
{
    public class NexportRegistrationFieldModel : BaseNopEntityModel, ILocalizedModel<NexportRegistrationFieldLocalizedModel>
    {
        public NexportRegistrationFieldModel()
        {
            AvailableFieldTypes = new List<SelectListItem>();
            AvailableFieldCategory = new List<SelectListItem>();

            StoreMappingIds = new List<int>();
            AvailableStores = new List<SelectListItem>();

            Locales = new List<NexportRegistrationFieldLocalizedModel>();
            RegistrationFieldOptionSearchModel = new NexportRegistrationFieldOptionSearchModel();
        }

        [NopResourceDisplayName("Plugins.Misc.Nexport.RegistrationField.Field.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.RegistrationField.Field.Type")]
        public NexportRegistrationFieldType Type { get; set; }

        public IList<SelectListItem> AvailableFieldTypes { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.RegistrationField.Field.CustomProfileFieldKey")]
        public string NexportCustomProfileFieldKey { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.RegistrationField.Field.Required")]
        public bool IsRequired { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.RegistrationField.Field.Active")]
        public bool IsActive { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.RegistrationField.Field.Category")]
        public int? FieldCategoryId { get; set; }

        public string FieldCategoryName { get; set; }

        public IList<SelectListItem> AvailableFieldCategory { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.RegistrationField.Field.Validation")]
        public bool Validation { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.RegistrationField.Field.ValidationRegex")]
        public string ValidationRegex { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.RegistrationField.Field.StoreMappings")]
        public string StoreMappings { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.RegistrationField.Field.Stores")]
        public IList<int> StoreMappingIds { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.RegistrationField.Field.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public IList<NexportRegistrationFieldLocalizedModel> Locales { get; set; }

        public NexportRegistrationFieldOptionSearchModel RegistrationFieldOptionSearchModel { get; set; }
    }

    public class NexportRegistrationFieldLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        public string Name { get; set; }
    }
}
