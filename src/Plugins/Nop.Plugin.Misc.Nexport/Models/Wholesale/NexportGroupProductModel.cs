using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.Rendering;
using NexportApi.Model;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using StackExchange.Redis;

namespace Nop.Plugin.Misc.Nexport.Models.Wholesale
{
    [SuppressMessage("ReSharper", "Mvc.TemplateNotResolved")]
    public record NexportGroupProductModel : BaseNopEntityModel
    {
        public string Name { get; set; }
        public int Available { get; set; } 
        public int Awaiting { get; set; } 
        public int Redeemed { get; set;} 
        public Guid GroupId { get; set; }
    }
}
