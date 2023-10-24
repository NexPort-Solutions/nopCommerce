using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;
using Permissions = System.Collections.Generic.HashSet<(string systemRoleName, Nop.Core.Domain.Security.PermissionRecord[] permissions)>;

namespace Nop.Plugin.Misc.Nexport.Services.Security;

public class PermissionProvider : IPermissionProvider
{
    public static readonly PermissionRecord ManageProductMapping = Make("Manage NexPort product mapping", nameof(ManageProductMapping));
    public static readonly PermissionRecord ManageSupplementalInfo = Make("Manage NexPort supplemental info", nameof(ManageSupplementalInfo));
    public static readonly PermissionRecord ManageOrderInvoice = Make("Manage NexPort order invoice", nameof(ManageOrderInvoice));
    public static readonly PermissionRecord ManageWholesaleRedemptions = Make("Manage NexPort wholesale redemptions", nameof(ManageWholesaleRedemptions));

    private static readonly PermissionRecord[] s_permissionRecords = new PermissionRecord[]
    {
        ManageProductMapping,
        ManageSupplementalInfo,
        ManageOrderInvoice,
        ManageWholesaleRedemptions,
    };

    public Permissions GetDefaultPermissions() => new(1) { (NopCustomerDefaults.AdministratorsRoleName, s_permissionRecords) };
    public IEnumerable<PermissionRecord> GetPermissions() => s_permissionRecords;
    private static PermissionRecord Make(string name, string system) => new() { Name = name, SystemName = system, Category = nameof(Nexport) };
}
