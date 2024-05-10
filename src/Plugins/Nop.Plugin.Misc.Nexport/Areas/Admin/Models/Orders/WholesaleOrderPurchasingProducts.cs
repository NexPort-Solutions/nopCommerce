using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Areas.Admin.Models.Orders;

public partial record WholesaleOrderPurchasingProducts : BaseNopModel
{
    //public List<ProductModel> Items { get; set; }

    public List<WholesaleOrderProductModel> Items { get; set; }
}

public partial record WholesaleOrderProductModel : ProductModel
{
    public int? NexportProductMappingId { get; set; }
}

public partial record WholesaleOrderProductListModel : BasePagedListModel<WholesaleOrderProductModel>
{
}

public partial class WholesaleOrderProduct : Product
{
    public WholesaleOrderProduct(Product product)
    {
        foreach (var prop in product.GetType().GetProperties())
        {
            GetType().GetProperty(prop.Name)?.SetValue(this, prop.GetValue(product, null), null);
        }
    }

    public int? NexportProductMappingId { get; set; }
}

public partial record WholesaleOrderPaymentInfoModel : BaseNopModel
{
    public Type PaymentViewComponent { get; set; }
}