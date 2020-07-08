using System;
using NexportApi.Model;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models.Syllabus
{
    public class NexportSyllabiResponseItemModel : BaseNopModel
    {
        public Guid SyllabusId { get; set; }

        public string Name { get; set; }

        public string UniqueName { get; set; }

        public GetSyllabiResponseItem.SyllabusTypeEnum? Type { get; set; }

        public Guid ProductId { get; set; }

        public int TotalMappings { get; set; }

        public Guid CatalogId { get; set; }

        public string OrgName { get; set; }

        public string OrgShortName { get; set; }

        public string SectionNumber { get; set; }
    }
}
