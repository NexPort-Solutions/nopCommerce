using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Common;

namespace Nop.Plugin.Misc.Nexport.Models.Customer
{
    public partial record CustomerNexportGroupModel : BaseNopModel
    {
        public CustomerNexportGroupModel()
        {
            //AdditionalProductReviewList = new List<ProductReviewReviewTypeMappingModel>();
        }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        
        //public int ProductId { get; set; }
        //public string ProductName { get; set; }
        //public string ProductSeName { get; set; }
        //public string Title { get; set; }
        //public string ReviewText { get; set; }
        //public string ReplyText { get; set; }
        //public int Rating { get; set; }
        //public string WrittenOnStr { get; set; }
        //public string ApprovalStatus { get; set; }
        //public IList<ProductReviewReviewTypeMappingModel> AdditionalProductReviewList { get; set; }
    }

    public partial record CustomerNexportGroupsModel : BaseNopModel
    {
        public CustomerNexportGroupsModel()
        {
            NexportGroups = new List<CustomerNexportGroupModel>();
        }

        public IList<CustomerNexportGroupModel> NexportGroups { get; set; }
        public PagerModel PagerModel { get; set; }

        #region Nested class

        /// <summary>
        /// record that has only page for route value. Used for (My Account) My Product Reviews pagination
        /// </summary>
        public partial record CustomerNexportGroupsRouteValues : IRouteValues
        {
            public int PageNumber { get; set; }
        }

        #endregion
    }
}