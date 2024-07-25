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
    [Migration(36, "Add redemption unassignment request")]
    [SkipMigration]
    public class M036_AddRedemptionUnassignmentRequest : Migration
    {
        private const string TABLE_NAME = "NexportRedemptionUnassignmentRequest";

        public override void Up()
        {
            Create.Table(TABLE_NAME)
                 .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("InvoiceItemId").AsGuid().NotNullable()
                .WithColumn("RequestedByCustomerId").AsInt32().NotNullable()
                 .WithColumn("CustomerComments").AsString().Nullable()
                 .WithColumn("StaffNotes").AsString().Nullable()
                 .WithColumn("ReasonForUnassignment").AsString().NotNullable()
                .WithColumn("RequestStatus").AsInt32().NotNullable()
                .WithColumn("UtcCreatedDate").AsDateTime2().NotNullable()
                .WithColumn("UtcLastModifiedDate").AsDateTime2().Nullable();
        }

        public override void Down()
        {
            Delete.Table(TABLE_NAME);
        }
    }
}
