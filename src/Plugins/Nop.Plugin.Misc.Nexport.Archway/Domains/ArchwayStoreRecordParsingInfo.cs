namespace Nop.Plugin.Misc.Nexport.Archway.Domains
{
    public class ArchwayStoreRecordParsingInfo : IArchwayStoreRecord
    {
        public int StoreNumber { get; set; }

        public string OperatorId { get; set; }

        public int RegionCode { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string PostalCode { get; set; }

        public string AdvertisingCoop { get; set; }

        public string StoreType { get; set; }

        public string OperatorFirstName { get; set; }

        public string OperatorLastName { get; set; }
    }
}
