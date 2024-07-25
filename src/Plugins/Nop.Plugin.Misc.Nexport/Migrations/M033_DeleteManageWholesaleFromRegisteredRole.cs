using System;
using FluentMigrator;
using LinqToDB;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(33, "Delete manage wholesale permission from the registered role")]
    [SkipMigration]
    public class M033_DeleteManageWholesaleFromRegisteredRole : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"Delete From PermissionRecord_Role_Mapping 
                Where CustomerRole_Id=(Select Id From CustomerRole Where SystemName='Registered') 
                AND PermissionRecord_Id=(Select Id From PermissionRecord Where SystemName='ManageNexportWholesale')");
        }

        public override void Down()
        {
        }
    }
}