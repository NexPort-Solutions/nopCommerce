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
        public static readonly PermissionRecord ManageNexportWholesaleRedemptions =
            new() { Name = "Manage Nexport wholesale redemptions", SystemName = "ManageNexportWholesaleRedemptions", Category = "Nexport" };

        public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string systemRoleName, PermissionRecord[] permissions)>
            {
                (NopCustomerDefaults.AdministratorsRoleName, new []
                {
                    ManageNexportProductMapping,
                    ManageSupplementalInfo,
                    ManageNexportOrderInvoice,
                    ManageNexportWholesaleRedemptions
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
                ManageNexportWholesaleRedemptions
            };
        }
    }
}
