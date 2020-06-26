using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Sale.CancelPendingOrderRequests.Models
{
    public class PendingOrderCancellationRequestSearchModel : BaseSearchModel
    {
        public PendingOrderCancellationRequestSearchModel()
        {
            RequestStatusList = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.PendingOrderCancellationRequests.SearchStartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Admin.PendingOrderCancellationRequests.SearchEndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [NopResourceDisplayName("Admin.PendingOrderCancellationRequests.RequestStatus")]
        public int RequestStatusId { get; set; }

        public IList<SelectListItem> RequestStatusList { get; set; }
    }
}