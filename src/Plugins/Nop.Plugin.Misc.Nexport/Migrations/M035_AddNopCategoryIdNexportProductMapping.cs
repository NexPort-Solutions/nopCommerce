using System;
using System.Xml.Serialization;
using FluentMigrator;
using FluentMigrator.SqlServer;
using LinqToDB;
using Nop.Core.Domain.Customers;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.Nexport.Domain;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Tags(NexportDefaults.PluginMigrationTag)]
    [Migration(35, "Add nop category id to product mapping")]
    [SkipMigration]
    public class M035_AddNopCategoryIdNexportProductMapping : Migration
    {
        

        public override void Up()
        {
            Alter.Table(nameof(NexportProductMapping)).AddColumn("NopCategoryId").AsInt32().Nullable();
        }

        public override void Down()
        {
            Delete.Column("NopCategoryId").FromTable(nameof(NexportProductMapping));
        }
    }
}
