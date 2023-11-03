using System;
using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(32, "Alter Wholesale Order Info GroupId Allow Null")]
    [SkipMigration]
    public class M032_AlterWholesaleOrderInfoGroupIdAllowNull : Migration
    {
        private const string TABLE_NAME = "WholesaleOrderInfo";

        public override void Up()
        {
            Alter.Table(TABLE_NAME).AlterColumn("NexportGroupId").AsGuid().Nullable();
        }

        public override void Down()
        {
            Alter.Table(TABLE_NAME).AlterColumn("NexportGroupId").AsGuid().NotNullable();
        }
    }
}
