using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.Nexport.Extensions;

namespace Nop.Plugin.Misc.Nexport.Domain.Enums;

public enum NexportOrderInvoiceItemRedemptionStatus
{
    [Display(Name = "Processing")]
    ProcessingAvailable = 0,

    [Display(Name = "Available")]
    Available = 1,

    [Display(Name = "Awaiting")]
    Awaiting = 2,

    [Display(Name = "Assigned")]
    Assigned = 3,

    [Display(Name = "Processing")]
    ProcessingAwaiting = 4
}

public static class NexportEnumExtensions
{
    public static SelectList ToNexportSelectListFromEnum(this NexportOrderInvoiceItemRedemptionStatus enumObj, bool markCurrentAsSelected = true, int[] valuesToExclude = null)
    {
        return enumObj.ToNexportSelectList(markCurrentAsSelected, valuesToExclude);
    }
}