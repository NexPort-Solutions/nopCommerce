using System;
using NexportApi.Model;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportSyllabiResponseItemModel : BaseNopModel
    {
        public Guid SyllabusId { get; set; }

        public string Name { get; set; }

        public GetSyllabiResponseItem.SyllabusTypeEnum? Type { get; set; }

        public Guid ProductId { get; set; }

        public int TotalMappings { get; set; }
    }
}
