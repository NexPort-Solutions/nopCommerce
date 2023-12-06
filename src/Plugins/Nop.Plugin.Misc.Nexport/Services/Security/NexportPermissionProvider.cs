using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.Nexport.Services.Security
{
    public class NexportPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageNexportProductMapping =
            new() { Name = "Manage Nexport product mapping", SystemName = nameof(ManageNexportProductMapping), Category = "Nexport" };
        public static readonly PermissionRecord ManageSupplementalInfo =
            new() { Name = "Manage Nexport supplemental info", SystemName = nameof(ManageSupplementalInfo), Category = "Nexport" };
        public static readonly PermissionRecord ManageNexportOrderInvoice =
            new() { Name = "Manage Nexport order invoice", SystemName = nameof(ManageNexportOrderInvoice), Category = "Nexport" };
        public static readonly PermissionRecord ManageNexportWholesaleRedemptions =
            new() { Name = "Manage Nexport wholesale redemptions", SystemName = nameof(ManageNexportWholesaleRedemptions), Category = "Nexport" };
        public static readonly PermissionRecord ManageNexportFundingPools =
            new() { Name = "Manage Nexport funding pools", SystemName = nameof(ManageNexportFundingPools), Category = "Nexport" };

        public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string systemRoleName, PermissionRecord[] permissions)>
            {
                (NopCustomerDefaults.AdministratorsRoleName, new []
                {
                    ManageNexportProductMapping,
                    ManageSupplementalInfo,
                    ManageNexportOrderInvoice,
                    ManageNexportWholesaleRedemptions,
                    ManageNexportFundingPools
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
                ManageNexportWholesaleRedemptions,
                ManageNexportFundingPools
            };
        }
    }
}
