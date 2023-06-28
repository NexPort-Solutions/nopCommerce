using System;
using System.Collections.Generic;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Common;

namespace Nop.Plugin.Misc.Nexport.Models.Customer
{
    public partial record CustomerNexportGroupProductModel : BaseNopModel
    {
        public CustomerNexportGroupProductModel()
        {
            
        }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        
       
    }

    public partial record CustomerNexportGroupProductsModel : BaseNopModel
    {
        public CustomerNexportGroupProductsModel()
        {
            NexportGroupProducts = new List<CustomerNexportGroupProductModel>();
        }

        public IList<CustomerNexportGroupProductModel> NexportGroupProducts { get; set; }

        public CustomerNexportGroupModel GroupModel { get; set; }

        public PagerModel PagerModel { get; set; }

        #region Nested class

        /// <summary>
        /// record that has only page for route value. Used for (My Account) My Product Reviews pagination
        /// </summary>
        public partial record CustomerNexportGroupProductsRouteValues : IRouteValues
        {
            public int PageNumber { get; set; }
        }

        #endregion
    }
}