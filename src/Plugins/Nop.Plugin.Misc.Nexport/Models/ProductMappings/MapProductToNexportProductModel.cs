using System;
using System.Collections.Generic;
using Nop.Plugin.Misc.Nexport.Domain.Enums;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.ProductMappings
{
    public class MapProductToNexportProductModel : BaseNopModel
    {
        public MapProductToNexportProductModel()
        {
            SelectedProductIds = new List<int>();
        }

        public IList<int> SelectedProductIds { get; set; }

        /// <summary>
        /// This is either CatalogId or the CatalogLinkId
        /// </summary>
        public Guid NexportProductId { get; set; }

        public Guid NexportCatalogId { get; set; }

        public Guid? NexportSyllabusId { get; set; }

        public NexportProductTypeEnum NexportProductType { get; set; }
    }
}
