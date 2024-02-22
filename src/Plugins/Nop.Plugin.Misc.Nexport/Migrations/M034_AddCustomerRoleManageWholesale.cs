using System;
using FluentMigrator;
using FluentMigrator.SqlServer;
using LinqToDB;
using Nop.Core.Domain.Customers;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(34, "Add customer role for manage wholesale")]
    [SkipMigration]
    public class M034_AddCustomerRoleManageWholesale : Migration
    {
        

        public override void Up()
        {
            Insert.IntoTable("CustomerRole").Row(new 
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
            });
        }

        public override void Down()
        {
            Delete.FromTable("CustomerRole").Row(new 
            {
                SystemName = "NexportWholesaleManager"
            });
        }
    }
}
