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
            Execute.Sql(@"
                INSERT INTO [dbo].[PermissionRecord_Role_Mapping]
                           ([PermissionRecord_Id]
                           ,[CustomerRole_Id])
                     VALUES
                           ((Select Id From PermissionRecord Where SystemName='ManageNexportWholesale')
                           ,(Select Id From CustomerRole Where SystemName='NexportWholesaleManager')");
        }

        public override void Down()
        {
            Delete.FromTable("CustomerRole").Row(new 
            {
                SystemName = "NexportWholesaleManager"
            });
            Execute.Sql(@"Delete From PermissionRecord_Role_Mapping 
                Where CustomerRole_Id=(Select Id From CustomerRole Where SystemName='NexportWholesaleManager') 
                AND PermissionRecord_Id=(Select Id From PermissionRecord Where SystemName='ManageNexportWholesale')");
        }
    }
}
