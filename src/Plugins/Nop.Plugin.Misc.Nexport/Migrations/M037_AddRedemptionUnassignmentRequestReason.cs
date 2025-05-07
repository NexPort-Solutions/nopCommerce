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
    [Migration(37, "Add redemption unassignment request reason")]
    [SkipMigration]
    public class M037_AddRedemptionUnassignmentRequestReason : Migration
    {
        private const string TABLE_NAME = "NexportRedemptionUnassignmentRequestReason";

        public override void Up()
        {
            Create.Table(TABLE_NAME)
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString(400).NotNullable()
                .WithColumn("DisplayOrder").AsInt32().NotNullable();

            Insert.IntoTable(TABLE_NAME)
                .Row(new { Name = "Accidentally Assigned", DisplayOrder = 1 });

            Insert.IntoTable(TABLE_NAME)
                .Row(new { Name = "Assigned to wrong customer", DisplayOrder = 2 });
        }

        public override void Down()
        {
            Delete.Table(TABLE_NAME);
        }
    }
}
