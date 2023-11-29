using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.Nexport.Services.Security
{
    public class NexportPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageNexportProductMapping =
            new() { Name = "Manage Nexport product mapping", SystemName = "ManageNexportProductMapping", Category = "Nexport" };
        public static readonly PermissionRecord ManageSupplementalInfo =
            new() { Name = "Manage Nexport supplemental info", SystemName = "ManageSupplementalInfo", Category = "Nexport" };
        public static readonly PermissionRecord ManageNexportOrderInvoice =
            new() { Name = "Manage Nexport order invoice", SystemName = "ManageNexportOrderInvoice", Category = "Nexport" };
        public static readonly PermissionRecord ManageNexportWholesalePurchases =
            new() { Name = NexportDefaults.CR_MANAGE_WHOLESALE_NAME, SystemName = NexportDefaults.CR_MANAGE_WHOLESALE_SYS_NAME, Category = "Nexport" };

        public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string systemRoleName, PermissionRecord[] permissions)>
            {
                (NopCustomerDefaults.AdministratorsRoleName, new []
                {
                    ManageNexportProductMapping,
                    ManageSupplementalInfo,
                    ManageNexportOrderInvoice,
                    ManageNexportWholesalePurchases
                }),
                (NopCustomerDefaults.RegisteredRoleName, new []
                {
                    ManageNexportWholesalePurchases
                })
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageNexportProductMapping,
                ManageSupplementalInfo,
                ManageNexportOrderInvoice,
                ManageNexportWholesalePurchases
            };
        }
    }
}
