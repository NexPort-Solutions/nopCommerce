using System;
using System.Linq;
using FluentMigrator;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Data;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(34, "Add customer role for manage wholesale")]
    [SkipMigration]
    public class M034_AddCustomerRoleManageWholesale : Migration
    {
        private readonly INopDataProvider _dataProvider;

        public M034_AddCustomerRoleManageWholesale(INopDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public override void Up()
        {
            if (!_dataProvider.GetTable<CustomerRole>().Any(cr => string.Compare(cr.SystemName, "NexportWholesaleManager", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var nexportWholesaleManagerCustomerRole = _dataProvider.InsertEntity(
                    new CustomerRole
                    {
                        Name = "Nexport Wholesale Manager",
                        SystemName = "NexportWholesaleManager",
                        FreeShipping = false,
                        TaxExempt = false,
                        Active = true,
                        IsSystemRole = true,
                        EnablePasswordLifetime = false,
                        OverrideTaxDisplayType = false,
                        DefaultTaxDisplayTypeId = 0,
                        PurchasedWithProductId = 0
                    }
                );

                if (!_dataProvider.GetTable<PermissionRecord>().Any(pr => string.Compare(pr.SystemName, "ManageNexportWholesale", StringComparison.InvariantCultureIgnoreCase) == 0))
                {
                    var manageNexportWholesalePermission = _dataProvider.InsertEntity(
                        new PermissionRecord
                        {
                            Name = NexportDefaults.MANAGE_NEXPORT_WHOLESALE_PERMISSION_NAME,
                            SystemName = NexportDefaults.MANAGE_NEXPORT_WHOLESALE_PERMISSION_SYSTEM_NAME,
                            Category = "Nexport"
                        }
                    );

                    var wholesaleManagerRole = _dataProvider
                        .GetTable<CustomerRole>()
                        .FirstOrDefault(x => x.IsSystemRole && x.SystemName == "NexportWholesaleManager");

                    _dataProvider.InsertEntity(
                        new PermissionRecordCustomerRoleMapping
                        {
                            CustomerRoleId = wholesaleManagerRole.Id,
                            PermissionRecordId = manageNexportWholesalePermission.Id
                        }
                    );
                }
            }
        }

        public override void Down()
        {
        }
    }
}