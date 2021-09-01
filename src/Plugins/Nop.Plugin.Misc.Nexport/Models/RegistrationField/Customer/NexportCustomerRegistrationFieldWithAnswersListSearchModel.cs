using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.RegistrationField.Customer
{
    public class NexportCustomerRegistrationFieldWithAnswersListSearchModel : BaseSearchModel
    {
        public NexportCustomerRegistrationFieldWithAnswersListSearchModel()
        {
            SetGridPageSize();

            AvailableStores = new List<SelectListItem>();
        }

        public int CustomerId { get; set; }

        public int StoreId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }
    }
}