using Nop.Web.Framework.Models.DataTables;

namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportDataTablesModel : DataTablesModel
    {
        public bool RowGrouping { get; set; }

        public string RowGroupingColumn { get; set; }

        public string CustomRowStartRender { get; set; }

        public string CustomRowEndRender { get; set; }
    }
}
