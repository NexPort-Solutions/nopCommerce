using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using NexportApi.Model;
using Nop.Web.Framework.Models;
using Nop.Plugin.Misc.Nexport.Domain;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportProductMappingModel : BaseNopEntityModel, INexportProductMapping
    {
        public NexportProductMappingModel()
        {
            GroupMembershipMappingModels = new List<NexportProductGroupMembershipMappingModel>();
        }

        public int NopProductId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.NexportProductName")]
        public string NexportProductName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.DisplayName")]
        public string DisplayName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.CatalogSyllabusLinkId")]
        public Guid? NexportCatalogSyllabusLinkId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.CatalogId")]
        public Guid NexportCatalogId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SyllabusId")]
        public Guid? NexportSyllabusId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SubscriptionOrgId")]
        public Guid? NexportSubscriptionOrgId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SubscriptionOrgName")]
        public string NexportSubscriptionOrgName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SubscriptionOrgShortName")]
        public string NexportSubscriptionOrgShortName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.Type")]
        public NexportProductTypeEnum Type { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.PublishingModel")]
        public CatalogResponseItem.PublishingModelEnum? PublishingModel { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.PricingModel")]
        public CatalogResponseItem.PricingModelEnum? PricingModel { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.ModifiedDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? UtcLastModifiedDate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.AvailableDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? UtcAvailableDate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.EndDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? UtcEndDate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.LastSynchronizationDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? UtcLastSynchronizationDate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.UtcAccessExpirationDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? UtcAccessExpirationDate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.AccessTimeLimit")]
        public long? AccessTimeLimit { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.CreditHours")]
        public decimal? CreditHours { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.UniqueName")]
        public string UniqueName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SectionCeus")]
        public string SectionCeus { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.SectionNumber")]
        public string SectionNumber { get;set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.IsSynchronized")]
        public bool IsSynchronized { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.AutoRedeem")]
        public bool AutoRedeem { get; set; }

        public int? StoreId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.StoreMapping")]
        public string StoreName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Nexport.IsExtensionProduct")]
        public bool IsExtensionProduct { get; set; }

        public NexportProductGroupMembershipMappingModel AddGroupMembershipMappingModel { get; set; }

        public IList<NexportProductGroupMembershipMappingModel> GroupMembershipMappingModels { get; set; }

        public bool Editable { get; set; }
    }
}
