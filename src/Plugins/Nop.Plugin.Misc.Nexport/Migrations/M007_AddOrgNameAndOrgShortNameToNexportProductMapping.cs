﻿using FluentMigrator;

namespace Nop.Plugin.Misc.Nexport.Migrations
{
    [Migration(7, "Add Nexport SubscriptionOrgName and SubscriptionOrgShortName to NexportProductMapping table")]
    public class M007_AddNexportSubscriptionOrgNameAndShortNameToNexportProductMapping : Migration
    {
        private const string TABLE_NAME = "NexportProductMapping";

        public override void Up()
        {
            Alter.Table(TABLE_NAME)
                .AddColumn("NexportSubscriptionOrgName").AsString().Nullable();
            Alter.Table(TABLE_NAME)
                .AddColumn("NexportSubscriptionOrgShortName").AsString(50).Nullable();
        }

        public override void Down()
        {
            Delete.Column("NexportSubscriptionOrgName").FromTable(TABLE_NAME);
            Delete.Column("NexportSubscriptionOrgShortName").FromTable(TABLE_NAME);
        }
    }
}