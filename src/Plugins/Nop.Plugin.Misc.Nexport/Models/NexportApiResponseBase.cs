namespace Nop.Plugin.Misc.Nexport.Models
{
    public class NexportApiResponseBase
    {
        public int TotalRecord { get; set; }

        public int RecordPerPage { get; set; }

        public int CurrentPage { get; set; }
    }
}