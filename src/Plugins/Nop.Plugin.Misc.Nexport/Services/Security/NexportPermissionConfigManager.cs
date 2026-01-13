using Nop.Core.Domain.Customers;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.Nexport.Services.Security;

public class NexportPermissionConfigManager : IPermissionConfigManager
{
    public const string MANAGE_NEXPORT_PRODUCT_MAPPING = "ManageNexportProductMapping";
    public const string MANAGE_SUPPLEMENTAL_INFO = "ManageSupplementalInfo";
    public const string MANAGE_NEXPORT_ORDER_INVOICE = "ManageNexportOrderInvoice";
    public const string MANAGE_NEXPORT_WHOLESALE = "ManageNexportWholesale";
    public const string MANAGE_NEXPORT_FUNDING_POOLS = "ManageNexportFundingPools";

    //public static readonly PermissionRecord ManageNexportProductMapping =
    //    new() { Name = "Manage Nexport product mapping", SystemName = "ManageNexportProductMapping", Category = "Nexport" };

    //public static readonly PermissionRecord ManageSupplementalInfo =
    //    new() { Name = "Manage Nexport supplemental info", SystemName = "ManageSupplementalInfo", Category = "Nexport" };

    //public static readonly PermissionRecord ManageNexportOrderInvoice =
    //    new() { Name = "Manage Nexport order invoice", SystemName = "ManageNexportOrderInvoice", Category = "Nexport" };

    //public static readonly PermissionRecord ManageNexportWholesale =
    //    new() { Name = NexportDefaults.MANAGE_NEXPORT_WHOLESALE_PERMISSION_NAME, SystemName = NexportDefaults.MANAGE_NEXPORT_WHOLESALE_PERMISSION_SYSTEM_NAME, Category = "Nexport" };

    //public static readonly PermissionRecord ManageNexportFundingPools =
    //    new() { Name = NexportDefaults.MANAGE_NEXPORT_FUNDING_POOLS_PERMISSION_NAME, SystemName = NexportDefaults.MANAGE_NEXPORT_FUNDING_POOLS_PERMISSION_SYSTEM_NAME, Category = "Nexport" };

    public IList<PermissionConfig> AllConfigs => new List<PermissionConfig>
    {
        new("Manage Nexport product mapping", MANAGE_NEXPORT_PRODUCT_MAPPING , "Nexport", NopCustomerDefaults.AdministratorsRoleName),
        new("Manage Nexport supplemental info", MANAGE_SUPPLEMENTAL_INFO , "Nexport", NopCustomerDefaults.AdministratorsRoleName),
        new("Manage Nexport order invoice", MANAGE_NEXPORT_ORDER_INVOICE , "Nexport", NopCustomerDefaults.AdministratorsRoleName),
        new(NexportDefaults.MANAGE_NEXPORT_WHOLESALE_PERMISSION_NAME, MANAGE_NEXPORT_WHOLESALE , "Nexport", NopCustomerDefaults.AdministratorsRoleName),
        new(NexportDefaults.MANAGE_NEXPORT_FUNDING_POOLS_PERMISSION_NAME, MANAGE_NEXPORT_FUNDING_POOLS, "Nexport", NopCustomerDefaults.AdministratorsRoleName)
    };
}