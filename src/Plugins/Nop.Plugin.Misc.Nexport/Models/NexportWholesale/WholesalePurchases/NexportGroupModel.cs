using System;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.NexportWholesale.WholesalePurchases;

public record NexportGroupModel : BaseNopEntityModel
{
    public new Guid? OrganizationId { get; set; }

    public Guid ParentId { get; set; }

    public string Name { get; set; }

    public string ShortName { get; set; }

    public int Type { get; set; }
}