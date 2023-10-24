namespace Nop.Plugin.Misc.Nexport.Models;

public record DataTablesModel : Web.Framework.Models.DataTables.DataTablesModel
{
    public bool RowGrouping { get; set; }

    public string? RowGroupingColumn { get; set; }

    public string? CustomRowStartRender { get; set; }

    public string? CustomRowEndRender { get; set; }
}
