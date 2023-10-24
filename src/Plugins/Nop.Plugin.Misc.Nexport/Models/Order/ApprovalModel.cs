using Nop.Web.Framework.Models;
using Nop.Web.Framework.Models.DataTables;

namespace Nop.Plugin.Misc.Nexport.Models.Order;

public record ApprovalModel : BaseNopModel
{
    public InvoiceItemSearchModel? SearchModel { get; set; }

    public static readonly ColumnProperty[] ColumnCollection = new ColumnProperty[]
    {
        new(nameof(InvoiceItemModel.ProductName))
        {
            Title = "Product",
            Width = "50",
        },
        new(nameof(InvoiceItemModel.ProductName))
        {
            Title = "Nexport Product",
            Width = "50",
        },
        new(nameof(InvoiceItemModel.SyllabusId))
        {
            Title = "Syllabus GroupGuid",
            Width = "100",
        },
        new(nameof(InvoiceItemModel.InvoiceItemId))
        {
            Title = "Invoice Item GroupGuid",
            Width = "100",
        },
        new(nameof(InvoiceItemModel.ExistingEnrollmentId))
        {
            Title = "Enrollment GroupGuid",
            Width = "100",
        },
        new(nameof(InvoiceItemModel.UtcExistingEnrollmentExpirationDate))
        {
            Title = "Enrollment Expiration",
            Width = "100",
            Render = new RenderCustom("renderLocalDateForTableEntry"),
        },
        new(nameof(InvoiceItemModel.Id))
        {
            Width = "200",
            ClassName = NopColumnClassDefaults.Button,
            Render = new RenderCustom("renderModifyEnrollment"),
            AutoWidth = false,
        },
    };
}
