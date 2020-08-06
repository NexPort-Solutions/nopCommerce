namespace Nop.Plugin.Misc.Nexport.Archway.Domains
{
    public interface IArchwayStoreRecord
    {
        int StoreNumber { get; set; }

        string OperatorId { get; set; }

        int RegionCode { get; set; }

        string Address { get; set; }

        string City { get; set; }

        string State { get; set; }

        string PostalCode { get; set; }

        string AdvertisingCoop { get; set; }

        string StoreType { get; set; }

        string OperatorFirstName { get; set; }

        string OperatorLastName { get; set; }
    }
}
