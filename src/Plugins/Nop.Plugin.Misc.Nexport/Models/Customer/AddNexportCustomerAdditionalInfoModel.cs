using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Customer
{
    public class AddNexportCustomerAdditionalInfoModel : BaseNopModel
    {
        public int CustomerId { get; set; }
        
        public int StoreId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public NexportCustomerRegistrationFieldAnswerListSearchModel NexportCustomerRegistrationFieldAnswerListSearchModel { get; set; }
    }
}